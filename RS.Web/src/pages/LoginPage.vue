<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NAlert, NButton, NCard, NForm, NFormItem, NInput } from 'naive-ui'
import { ApiError } from '@/api'
import { loadCurrentUser, signIn } from '@/auth'

const route = useRoute()
const router = useRouter()

const username = ref('')
const password = ref('')
const signingIn = ref(false)
const errorMessage = ref('')

// Back to the page that asked for sign-in; only paths inside the app.
function returnPath() {
  const target = route.query.returnTo
  return typeof target === 'string' && target.startsWith('/') && !target.startsWith('//') ? target : '/inbox'
}

onMounted(async () => {
  if (await loadCurrentUser()) {
    await router.replace(returnPath())
  }
})

async function onSubmit() {
  errorMessage.value = ''
  signingIn.value = true
  try {
    await signIn(username.value.trim(), password.value)
    await router.replace(returnPath())
  } catch (error) {
    errorMessage.value =
      error instanceof ApiError && error.status === 401
        ? 'Kullanıcı adı veya şifre hatalı.'
        : 'Giriş yapılamadı. Lütfen biraz sonra tekrar deneyin.'
  } finally {
    signingIn.value = false
  }
}
</script>

<template>
  <div class="login-screen">
    <n-card class="login-card">
      <h1 class="title">Direniş Takip</h1>
      <p class="subtitle">Devam etmek için giriş yapın.</p>
      <n-form @submit.prevent="onSubmit">
        <n-form-item label="Kullanıcı adı" label-placement="top">
          <n-input
            v-model:value="username"
            :input-props="{ autocomplete: 'username', id: 'username' }"
            placeholder="Kullanıcı adınız"
          />
        </n-form-item>
        <n-form-item label="Şifre" label-placement="top">
          <n-input
            v-model:value="password"
            type="password"
            show-password-on="click"
            :input-props="{ autocomplete: 'current-password', id: 'password' }"
            placeholder="Şifreniz"
          />
        </n-form-item>
        <n-alert v-if="errorMessage" type="error" :show-icon="false" class="error">{{ errorMessage }}</n-alert>
        <n-button
          type="primary"
          attr-type="submit"
          block
          size="large"
          :loading="signingIn"
          :disabled="!username.trim() || !password"
        >
          Giriş yap
        </n-button>
      </n-form>
    </n-card>
  </div>
</template>

<style scoped>
.login-screen {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px 16px;
  box-sizing: border-box;
}
.login-card {
  width: 100%;
  max-width: 400px;
}
.title {
  margin: 0;
  font-size: 26px;
}
.subtitle {
  margin: 6px 0 20px;
  color: var(--rs-muted);
}
.error {
  margin-bottom: 16px;
}
</style>
