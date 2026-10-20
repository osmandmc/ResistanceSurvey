import { createRouter, createWebHistory } from 'vue-router'
import { setUnauthorizedHandler } from './api'
import { forgetCurrentUser, loadCurrentUser } from './auth'
import AppLayout from './layouts/AppLayout.vue'
import ArchivePage from './pages/ArchivePage.vue'
import CaseDetailPage from './pages/CaseDetailPage.vue'
import CasesPage from './pages/CasesPage.vue'
import DefinitionsPage from './pages/DefinitionsPage.vue'
import InboxPage from './pages/InboxPage.vue'
import LoginPage from './pages/LoginPage.vue'
import ReviewPage from './pages/ReviewPage.vue'
import StatusPage from './pages/StatusPage.vue'
import UploadPage from './pages/UploadPage.vue'
import NewCasePage from './pages/NewCasePage.vue'
import NotFoundPage from './pages/NotFoundPage.vue'

declare module 'vue-router' {
  interface RouteMeta {
    /** Reachable without signing in. */
    public?: boolean
    /** Page heading and browser tab title (shown to users, so Turkish). */
    title?: string
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
        { path: 'archive', component: ArchivePage, meta: { title: 'Haber Arşivi' } },
        { path: 'cases', component: CasesPage, meta: { title: 'Vakalar' } },
        { path: 'cases/new', component: NewCasePage, meta: { title: 'Yeni vaka' } },
        { path: 'cases/:id(\\d+)', component: CaseDetailPage, meta: { title: 'Vaka' } },
        { path: 'status', component: StatusPage, meta: { title: 'Durum Paneli' } },
        { path: 'upload', component: UploadPage, meta: { title: 'Yükleme' } },
        { path: 'definitions', redirect: '/definitions/corporations' },
        { path: 'definitions/:kind', component: DefinitionsPage, meta: { title: 'Tanımlar' } },
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
