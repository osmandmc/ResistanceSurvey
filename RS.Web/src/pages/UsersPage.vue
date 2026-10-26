<script setup lang="ts">
import { ref } from 'vue'
import { NAlert, NButton, NEmpty, NInput, NPopconfirm, NSelect, NSpin, NTag, useMessage } from 'naive-ui'
import { errorMessage } from '@/api'
import { currentUser } from '@/auth'
import { formatDate, formatDateTime } from '@/format'
import { changeUser, createUser, linkFor, listUsers, newLink, type LinkResult, type UserRow, type UserState } from '@/usersApi'

// Tanımlar → Kullanıcılar (F4a), yönetici only. A person is invited with a one-time link that the yönetici sends
// them directly; the same kind of link replaces a forgotten password. The app sends no e-mail.
const message = useMessage()
const users = ref<UserRow[] | null>(null)
const loadError = ref('')

const roles = [
  { label: 'Araştırmacı', value: 'User' },
  { label: 'Yönetici', value: 'Admin' },
]
const roleLabel = (role: string) => roles.find((r) => r.value === role)?.label ?? role

const states: Record<UserState, { label: string; type: 'success' | 'info' | 'warning' | 'default' }> = {
  active: { label: 'Etkin', type: 'success' },
  invited: { label: 'Davet gönderildi', type: 'info' },
  linkExpired: { label: 'Bağlantının süresi doldu', type: 'warning' },
  disabled: { label: 'Devre dışı', type: 'default' },
}

async function load() {
  loadError.value = ''
  try {
    users.value = await listUsers()
  } catch (error) {
    loadError.value = errorMessage(error, 'Kullanıcılar yüklenemedi.')
  }
}
void load()

// The link just made, shown once (only its hash is stored).
const shownLink = ref<{ username: string; link: string; reset: boolean } | null>(null)

function show(result: LinkResult, reset: boolean) {
  if (result.token) shownLink.value = { username: result.user.username, link: linkFor(result.token), reset }
}

async function copyLink() {
  if (!shownLink.value) return
  try {
    await navigator.clipboard.writeText(shownLink.value.link)
    message.success('Bağlantı kopyalandı.')
  } catch {
    message.warning('Kopyalanamadı; bağlantıyı seçip kopyalayın.')
  }
}

const username = ref('')
const role = ref('User')
const creating = ref(false)

async function invite() {
  creating.value = true
  try {
    show(await createUser(username.value.trim(), role.value), false)
    username.value = ''
    role.value = 'User'
    await load()
  } catch (error) {
    message.error(errorMessage(error, 'Kullanıcı eklenemedi.'))
  } finally {
    creating.value = false
  }
}

const busyId = ref<number | null>(null)

async function act(user: UserRow, action: () => Promise<LinkResult>, fallback: string, reset = false) {
  busyId.value = user.id
  try {
    show(await action(), reset)
    await load()
  } catch (error) {
    message.error(errorMessage(error, fallback))
  } finally {
    busyId.value = null
  }
}

const isMe = (user: UserRow) => user.id === currentUser.value?.id
</script>

<template>
  <p class="back"><router-link to="/definitions">← Tanımlar</router-link></p>
  <h1 class="heading">Kullanıcılar</h1>
  <p class="description">
    Yeni bir kişiyi davet edin: uygulama tek kullanımlık bir bağlantı verir, siz de kişiye doğrudan gönderirsiniz
    (e-posta gönderilmez). Kişi şifresini bu bağlantıyla kendisi belirler. Şifresini unutan biri için
    <strong>Yeni bağlantı</strong> verin.
  </p>

  <section class="panel">
    <h2 class="panel-title">Davet et</h2>
    <form class="invite" @submit.prevent="invite">
      <n-input v-model:value="username" placeholder="Kullanıcı adı, ör. ayse.k" :input-props="{ 'aria-label': 'Kullanıcı adı', autocomplete: 'off' }" class="username" />
      <n-select v-model:value="role" :options="roles" class="role" aria-label="Rol" />
      <n-button type="primary" attr-type="submit" :loading="creating" :disabled="!username.trim()">Davet bağlantısı oluştur</n-button>
    </form>
    <p class="small">Araştırmacılar uygulamanın her yerini kullanır; yöneticiler ayrıca bu sayfadaki hesapları yönetir.</p>
  </section>

  <n-alert v-if="shownLink" type="success" closable class="link" @close="shownLink = null">
    <template #header>{{ shownLink.reset ? 'Yeni bağlantı' : 'Davet bağlantısı' }}: {{ shownLink.username }}</template>
    <div class="link-row">
      <code class="link-text">{{ shownLink.link }}</code>
      <n-button size="small" @click="copyLink">Kopyala</n-button>
    </div>
    <p class="small">
      Bu bağlantı yalnızca şimdi gösterilir. Kişiye doğrudan gönderin; 7 gün geçerlidir ve bir kez kullanılabilir.
      <template v-if="shownLink.reset"> Önceki bağlantı artık çalışmaz; mevcut şifre, yenisi belirlenene kadar geçerlidir.</template>
    </p>
  </n-alert>

  <section class="panel">
    <n-alert v-if="loadError" type="error" :show-icon="false">
      {{ loadError }} <n-button size="small" @click="load">Tekrar dene</n-button>
    </n-alert>
    <div v-else-if="!users" class="state"><n-spin /></div>
    <div v-else-if="users.length === 0" class="state"><n-empty description="Henüz kullanıcı yok." /></div>
    <div v-else class="table-wrap">
      <table class="users-table">
        <thead>
          <tr>
            <th>Kullanıcı adı</th>
            <th>Rol</th>
            <th>Durum</th>
            <th>Son giriş</th>
            <th><span class="visually-hidden">İşlemler</span></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in users" :key="user.id" :class="{ disabled: user.state === 'disabled' }">
            <td class="name">
              {{ user.username }}
              <span v-if="isMe(user)" class="me">(siz)</span>
              <div class="small muted">{{ formatDate(user.createdAt) }}{{ user.createdBy ? ` · ${user.createdBy}` : '' }}</div>
            </td>
            <td>
              <span v-if="isMe(user)">{{ roleLabel(user.role) }}</span>
              <n-select
                v-else
                :value="user.role"
                :options="roles"
                size="small"
                class="role"
                :disabled="busyId === user.id || user.state === 'disabled'"
                :aria-label="`${user.username} rolü`"
                @update:value="(r: string) => act(user, () => changeUser(user.id, { role: r }), 'Rol değiştirilemedi.')"
              />
            </td>
            <td>
              <n-tag :type="states[user.state].type" size="small" :bordered="false">{{ states[user.state].label }}</n-tag>
              <div v-if="user.linkExpiresAt" class="small muted">{{ formatDate(user.linkExpiresAt) }} tarihine kadar</div>
            </td>
            <td>{{ formatDateTime(user.lastSignInAt) }}</td>
            <td class="actions">
              <template v-if="!isMe(user)">
                <n-button
                  v-if="user.state !== 'disabled'"
                  size="small"
                  :disabled="busyId === user.id"
                  @click="act(user, () => newLink(user.id), 'Bağlantı oluşturulamadı.', true)"
                >
                  Yeni bağlantı
                </n-button>
                <n-popconfirm
                  v-if="user.state !== 'disabled'"
                  positive-text="Devre dışı bırak"
                  negative-text="Vazgeç"
                  @positive-click="act(user, () => changeUser(user.id, { disabled: true }), 'Hesap devre dışı bırakılamadı.')"
                >
                  <template #trigger>
                    <n-button size="small" :disabled="busyId === user.id">Devre dışı bırak</n-button>
                  </template>
                  {{ user.username }} giriş yapamaz; açık oturumu hemen kapanır. Kayıtlarındaki adı kalır.
                </n-popconfirm>
                <n-button
                  v-else
                  size="small"
                  :disabled="busyId === user.id"
                  @click="act(user, () => changeUser(user.id, { disabled: false }), 'Hesap etkinleştirilemedi.')"
                >
                  Etkinleştir
                </n-button>
              </template>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<style scoped>
.back {
  margin: 0 0 8px;
  font-size: 14px;
}
.back a {
  color: var(--rs-accent);
}
.heading {
  margin: 0;
  font-size: 30px;
}
.description {
  margin: 6px 0 20px;
  color: var(--rs-muted);
  font-size: 16px;
  max-width: 760px;
}
.panel {
  background: var(--rs-surface);
  border: 1px solid var(--rs-border);
  border-radius: 10px;
  padding: 16px 20px;
  margin-bottom: 16px;
}
.panel-title {
  margin: 0 0 12px;
  font-size: 18px;
}
.invite {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.username {
  flex: 1 1 220px;
  max-width: 320px;
}
.role {
  width: 150px;
}
.small {
  margin: 8px 0 0;
  font-size: 13px;
}
.muted {
  color: var(--rs-muted);
  margin: 2px 0 0;
}
.link {
  margin-bottom: 16px;
}
.link-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
}
.link-text {
  word-break: break-all;
  font-size: 13px;
  padding: 6px 8px;
  border-radius: 6px;
  background: var(--rs-ground);
  border: 1px solid var(--rs-border);
  user-select: all;
}
.state {
  padding: 40px 16px;
}
.table-wrap {
  overflow-x: auto;
}
.users-table {
  width: 100%;
  min-width: 720px;
  border-collapse: collapse;
  font-size: 15px;
}
.users-table th {
  text-align: left;
  font-size: 12px;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--rs-muted);
  font-weight: 600;
  padding: 10px 16px;
  border-bottom: 1px solid var(--rs-border);
}
.users-table td {
  padding: 10px 16px;
  border-bottom: 1px solid #eef0f2;
  vertical-align: middle;
}
.users-table tr.disabled {
  color: var(--rs-muted);
}
.name {
  font-weight: 500;
}
.me {
  font-weight: 400;
  color: var(--rs-muted);
  margin-left: 4px;
}
.actions {
  text-align: right;
  white-space: nowrap;
}
.actions > * + * {
  margin-left: 8px;
}
.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  overflow: hidden;
  clip: rect(0 0 0 0);
}
</style>
