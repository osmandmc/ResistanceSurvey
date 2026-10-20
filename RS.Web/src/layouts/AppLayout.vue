<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { NButton } from 'naive-ui'
import { currentUser, displayName, signOut } from '@/auth'

const router = useRouter()

// Menu labels are shown to users, so they are Turkish; paths stay English.
const menu = [
  { path: '/inbox', label: 'Gelen Kutusu' },
  { path: '/archive', label: 'Haber Arşivi' },
  { path: '/cases', label: 'Vakalar' },
  { path: '/status', label: 'Durum Paneli' },
  { path: '/upload', label: 'Yükleme' },
]

const userName = computed(() => (currentUser.value ? displayName(currentUser.value) : ''))

async function onSignOut() {
  await signOut()
  await router.replace('/login')
}
</script>

<template>
  <header class="top-bar">
    <span class="brand">Direniş Takip</span>
    <nav class="menu" aria-label="Ana menü">
      <router-link v-for="item in menu" :key="item.path" :to="item.path" class="menu-link" active-class="active">
        {{ item.label }}
      </router-link>
    </nav>
    <div class="account">
      <!-- The old screens stay reachable until each is replaced. -->
      <a class="old-screens" href="/Resistance/IndexVue">Eski ekranlar</a>
      <span class="user-name">{{ userName }}</span>
      <n-button size="small" secondary class="sign-out" @click="onSignOut">Çıkış</n-button>
    </div>
  </header>
  <main class="content">
    <router-view />
  </main>
</template>

<style scoped>
.top-bar {
  background: var(--rs-brand);
  padding: 12px 24px;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px 32px;
}
.brand {
  color: #fff;
  font-weight: 700;
  font-size: 18px;
}
.menu {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}
.menu-link {
  color: #d7e3e3;
  text-decoration: none;
  padding: 10px 14px;
  border-radius: 6px;
  font-weight: 500;
  font-size: 15px;
}
.menu-link:hover {
  background: #1f3a3a;
  color: #fff;
}
.menu-link.active {
  background: #fff;
  color: var(--rs-brand);
}
.account {
  margin-left: auto;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px 16px;
  color: #b8caca;
  font-size: 14px;
}
.old-screens {
  color: #b8caca;
}
.old-screens:hover {
  color: #fff;
}
.user-name {
  color: #fff;
}
.content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 32px 24px 48px;
}
@media (max-width: 600px) {
  .top-bar,
  .content {
    padding-left: 16px;
    padding-right: 16px;
  }
}
</style>
