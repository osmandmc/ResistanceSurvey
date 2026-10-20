<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { NAlert, NButton, NCheckbox, NEmpty, NRadio, NSpin, NTag, useMessage } from 'naive-ui'
import { errorMessage } from '@/api'
import { dismissDuplicates, listDuplicates, mergeGroup, type DefinitionKind, type DuplicateGroup } from '@/definitionsApi'

// "Olası tekrarlar": groups of look-alike names. Per group, choose the value to keep and which others to merge
// into it, or mark the group "aynı değil".
const props = defineProps<{ kind: DefinitionKind; unit: 'vaka' | 'eylem' }>()
const emit = defineEmits<{ changed: [] }>()
const message = useMessage()

const groups = ref<DuplicateGroup[]>([])
const loading = ref(false)
const loadError = ref('')
// Per group (by key): the value to keep, and the values included in the merge.
const keep = reactive<Record<string, number>>({})
const included = reactive<Record<string, number[]>>({})
const busy = ref<string | null>(null)

async function load() {
  loading.value = true
  loadError.value = ''
  try {
    groups.value = await listDuplicates(props.kind)
    for (const group of groups.value) {
      keep[group.key] = group.items[0].id
      included[group.key] = group.items.map((i) => i.id)
    }
  } catch (error) {
    loadError.value = errorMessage(error, 'Olası tekrarlar yüklenemedi.')
  } finally {
    loading.value = false
  }
}
watch(() => props.kind, load, { immediate: true })

function toggle(group: DuplicateGroup, id: number, on: boolean) {
  const list = included[group.key]
  included[group.key] = on ? [...list, id] : list.filter((x) => x !== id)
}

const toMerge = (group: DuplicateGroup) => included[group.key].filter((id) => id !== keep[group.key])

function done(group: DuplicateGroup) {
  groups.value = groups.value.filter((g) => g.key !== group.key)
  emit('changed')
}

async function merge(group: DuplicateGroup) {
  busy.value = group.key
  try {
    const result = await mergeGroup(props.kind, keep[group.key], toMerge(group))
    const kept = group.items.find((i) => i.id === keep[group.key])!.name
    message.success(`“${kept}” altında birleştirildi: ${result.affected} ${props.unit} güncellendi.`)
    done(group)
  } catch (error) {
    message.error(errorMessage(error, 'Birleştirilemedi.'))
  } finally {
    busy.value = null
  }
}

async function dismiss(group: DuplicateGroup) {
  busy.value = group.key
  try {
    await dismissDuplicates(props.kind, group.items.map((i) => i.id))
    done(group)
  } catch (error) {
    message.error(errorMessage(error, 'Kaydedilemedi.'))
  } finally {
    busy.value = null
  }
}
</script>

<template>
  <div v-if="loading" class="state"><n-spin /></div>
  <n-alert v-else-if="loadError" type="error" :show-icon="false" class="state-alert">
    {{ loadError }} <n-button size="small" @click="load()">Tekrar dene</n-button>
  </n-alert>
  <div v-else-if="groups.length === 0" class="state"><n-empty description="Olası tekrar yok." /></div>

  <template v-else>
    <p class="intro">
      {{ groups.length }} grup. Adları büyük-küçük harf, noktalama, Türkçe harfler<template v-if="kind === 'companies'"> ve A.Ş., Ltd. Şti. gibi ekler</template>
      dışında aynı olan değerler. Her grupta tutulacak değeri seçin; işaretli diğerleri onunla birleştirilir.
    </p>
    <ul class="groups">
      <li v-for="group in groups" :key="group.key" class="group">
        <table class="group-table">
          <thead>
            <tr>
              <th>Tut</th>
              <th>Birleştir</th>
              <th>Ad</th>
              <th class="num">Kullanım</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in group.items" :key="item.id" :class="{ kept: keep[group.key] === item.id }">
              <td>
                <n-radio
                  :checked="keep[group.key] === item.id"
                  :name="`keep-${group.key}`"
                  :aria-label="`${item.name} tutulsun`"
                  @update:checked="() => (keep[group.key] = item.id)"
                />
              </td>
              <td>
                <n-checkbox
                  :checked="keep[group.key] === item.id || included[group.key].includes(item.id)"
                  :disabled="keep[group.key] === item.id"
                  :aria-label="`${item.name} birleştirilsin`"
                  @update:checked="(on: boolean) => toggle(group, item.id, on)"
                />
              </td>
              <td class="name">
                {{ item.name }}
                <n-tag v-if="item.approved === false" size="small" type="warning" :bordered="false">onay bekliyor</n-tag>
              </td>
              <td class="num">{{ item.usage }} {{ unit }}</td>
            </tr>
          </tbody>
        </table>
        <div class="group-actions">
          <n-button size="small" @click="dismiss(group)" :disabled="busy !== null">Aynı değil</n-button>
          <n-button size="small" type="primary" :disabled="toMerge(group).length === 0 || busy !== null" :loading="busy === group.key" @click="merge(group)">
            {{ toMerge(group).length }} değeri birleştir
          </n-button>
        </div>
      </li>
    </ul>
  </template>
</template>

<style scoped>
.state {
  padding: 40px 16px;
}
.state-alert {
  margin: 16px;
}
.intro {
  margin: 0;
  padding: 12px 16px;
  font-size: 14px;
  color: var(--rs-muted);
}
.groups {
  list-style: none;
  margin: 0;
  padding: 0 16px 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.group {
  border: 1px solid var(--rs-border);
  border-radius: 8px;
  overflow-x: auto;
}
.group-table {
  width: 100%;
  min-width: 480px;
  border-collapse: collapse;
  font-size: 15px;
}
.group-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 8px 12px;
  border-bottom: 1px solid var(--rs-border);
}
.group-table td {
  padding: 8px 12px;
  border-bottom: 1px solid #eef0f2;
}
.group-table th:nth-child(-n + 2),
.group-table td:nth-child(-n + 2) {
  width: 72px;
}
.group-table tr.kept {
  background: #f0f8f7;
}
.name {
  font-weight: 500;
}
.name :deep(.n-tag) {
  margin-left: 8px;
}
.num {
  text-align: right;
  white-space: nowrap;
  font-variant-numeric: tabular-nums;
}
.group-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  padding: 8px 12px;
}
</style>
