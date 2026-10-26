<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NCard, NForm, NFormItem, NInput, NSpin } from 'naive-ui'
import { errorMessage } from '@/api'
import { setCurrentUser } from '@/auth'
import { acceptInvitation, getInvitation, type Invitation } from '@/usersApi'

// The invitation link (F4a): /app/welcome?token=…. The person sets their own password and is signed in.
const route = useRoute()
const router = useRouter()

const minLength = 10
const token = computed(() => (typeof route.query.token === 'string' ? route.query.token : ''))
const invitation = ref<Invitation | null>(null)
const loading = ref(true)
const linkError = ref('')

const password = ref('')
const repeat = ref('')
const saving = ref(false)
const saveError = ref('')

const tooShort = computed(() => password.value.length > 0 && password.value.length < minLength)
const mismatch = computed(() => repeat.value.length > 0 && repeat.value !== password.value)
const canSave = computed(() => password.value.length >= minLength && repeat.value === password.value)

onMounted(async () => {
  try {
    invitation.value = token.value ? await getInvitation(token.value) : null
    if (!invitation.value) linkError.value = 'Bu bağlantı artık geçerli değil; yöneticiden yeni bir bağlantı isteyin.'
  } catch (error) {
    linkError.value = errorMessage(error, 'Bağlantı denetlenemedi. Lütfen biraz sonra tekrar deneyin.')
  } finally {
    loading.value = false
  }
})

async function save() {
  saveError.value = ''
  saving.value = true
  try {
    setCurrentUser(await acceptInvitation(token.value, password.value))
    await router.replace('/inbox')
  } catch (error) {
    saveError.value = errorMessage(error, 'Şifre kaydedilemedi. Lütfen biraz sonra tekrar deneyin.')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="welcome-screen">
    <n-card class="welcome-card">
      <h1 class="title">Direniş Takip</h1>
      <div v-if="loading" class="state"><n-spin /></div>
      <template v-else-if="linkError">
        <n-alert type="warning" :show-icon="false">{{ linkError }}</n-alert>
        <p class="to-login"><router-link to="/login">Giriş sayfasına git</router-link></p>
      </template>
      <template v-else-if="invitation">
        <p class="subtitle">
          <strong>{{ invitation.username }}</strong> hesabı için
          {{ invitation.hasPassword ? 'yeni bir şifre belirleyin.' : 'şifrenizi belirleyin.' }}
          Girişte bu kullanıcı adını ve şifreyi kullanacaksınız.
        </p>
        <!-- The username lets password managers save the pair. -->
        <input type="text" name="username" autocomplete="username" :value="invitation.username" readonly hidden />
        <n-form @submit.prevent="save">
          <n-form-item
            label="Şifre"
            label-placement="top"
            :validation-status="tooShort ? 'error' : undefined"
            :feedback="tooShort ? `En az ${minLength} karakter.` : `En az ${minLength} karakter; bir cümle de olur.`"
          >
            <n-input v-model:value="password" type="password" show-password-on="click" :input-props="{ autocomplete: 'new-password', id: 'password' }" />
          </n-form-item>
          <n-form-item
            label="Şifre (tekrar)"
            label-placement="top"
            :validation-status="mismatch ? 'error' : undefined"
            :feedback="mismatch ? 'İki şifre aynı değil.' : undefined"
          >
            <n-input v-model:value="repeat" type="password" show-password-on="click" :input-props="{ autocomplete: 'new-password', id: 'repeat' }" />
          </n-form-item>
          <n-alert v-if="saveError" type="error" :show-icon="false" class="error">{{ saveError }}</n-alert>
          <n-button type="primary" attr-type="submit" block size="large" :loading="saving" :disabled="!canSave">Şifreyi kaydet ve giriş yap</n-button>
        </n-form>
      </template>
    </n-card>
  </div>
</template>

<style scoped>
.welcome-screen {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px 16px;
  box-sizing: border-box;
}
.welcome-card {
  width: 100%;
  max-width: 420px;
}
.title {
  margin: 0;
  font-size: 26px;
}
.subtitle {
  margin: 6px 0 20px;
  color: var(--rs-muted);
}
.state {
  padding: 24px 0;
}
.to-login {
  margin: 16px 0 0;
}
.error {
  margin-bottom: 16px;
}
</style>
