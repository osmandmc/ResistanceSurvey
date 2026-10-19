import { createRouter, createWebHistory } from 'vue-router'
import { setUnauthorizedHandler } from './api'
import { forgetCurrentUser, loadCurrentUser } from './auth'
import AppLayout from './layouts/AppLayout.vue'
import InboxPage from './pages/InboxPage.vue'
import LoginPage from './pages/LoginPage.vue'
import ReviewPage from './pages/ReviewPage.vue'
import NotFoundPage from './pages/NotFoundPage.vue'
import PlaceholderPage from './pages/PlaceholderPage.vue'

declare module 'vue-router' {
  interface RouteMeta {
    /** Reachable without signing in. */
    public?: boolean
    /** Page heading and browser tab title (shown to users, so Turkish). */
    title?: string
    /** One line on what the page is for, until it is built. */
    description?: string
  }
}

export const router = createRouter({
  history: createWebHistory('/app/'),
  routes: [
    { path: '/login', component: LoginPage, meta: { public: true, title: 'Giriş' } },
    {
      path: '/',
      component: AppLayout,
      children: [
        { path: '', redirect: '/inbox' },
        { path: 'inbox', component: InboxPage, meta: { title: 'Gelen Kutusu' } },
        { path: 'review/:articleId', component: ReviewPage, meta: { title: 'İnceleme' } },
        {
          path: 'archive',
          component: PlaceholderPage,
          meta: { title: 'Haber Arşivi', description: 'Her durumdaki tüm haberler ve her haberin geçmişi.' },
        },
        {
          path: 'cases',
          component: PlaceholderPage,
          meta: { title: 'Vakalar', description: 'Direnişler ve eylemler, bağlı haberleriyle birlikte.' },
        },
        {
          path: 'status',
          component: PlaceholderPage,
          meta: { title: 'Durum Paneli', description: 'Haberlerin aşamalara göre sayıları ve hatalar.' },
        },
        {
          path: 'upload',
          component: PlaceholderPage,
          meta: { title: 'Yükleme', description: 'Excel ile haber yükleme.' },
        },
        { path: ':pathMatch(.*)*', component: NotFoundPage, meta: { title: 'Sayfa bulunamadı' } },
      ],
    },
  ],
})

// Every page except the login page needs a signed-in user.
router.beforeEach(async (to) => {
  if (to.meta.public) {
    return true
  }
  const user = await loadCurrentUser()
  return user ? true : { path: '/login', query: { returnTo: to.fullPath } }
})

router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} · Direniş Takip` : 'Direniş Takip'
})

// A 401 from the API means the session expired: ask for sign-in, then come back here.
setUnauthorizedHandler(() => {
  forgetCurrentUser()
  const returnTo = router.currentRoute.value.fullPath
  void router.push({ path: '/login', query: { returnTo } })
})
