import { createRouter, createWebHistory } from 'vue-router'
import { setUnauthorizedHandler } from './api'
import { forgetCurrentUser, loadCurrentUser } from './auth'
import AppLayout from './layouts/AppLayout.vue'
import ActivityPage from './pages/ActivityPage.vue'
import ArchivePage from './pages/ArchivePage.vue'
import CaseDetailPage from './pages/CaseDetailPage.vue'
import CasesPage from './pages/CasesPage.vue'
import CompanyPage from './pages/CompanyPage.vue'
import DefinitionsPage from './pages/DefinitionsPage.vue'
import InboxPage from './pages/InboxPage.vue'
import LoginPage from './pages/LoginPage.vue'
import ReviewPage from './pages/ReviewPage.vue'
import StatusPage from './pages/StatusPage.vue'
import UploadPage from './pages/UploadPage.vue'
import UsersPage from './pages/UsersPage.vue'
import WelcomePage from './pages/WelcomePage.vue'
import NewCasePage from './pages/NewCasePage.vue'
import NotFoundPage from './pages/NotFoundPage.vue'
import PromptsPage from './pages/PromptsPage.vue'

declare module 'vue-router' {
  interface RouteMeta {
    /** Reachable without signing in. */
    public?: boolean
    /** Yönetici only (F4a). */
    admin?: boolean
    /** Page heading and browser tab title (shown to users, so Turkish). */
    title?: string
  }
}

export const router = createRouter({
  history: createWebHistory('/app/'),
  routes: [
    { path: '/login', component: LoginPage, meta: { public: true, title: 'Giriş' } },
    { path: '/welcome', component: WelcomePage, meta: { public: true, title: 'Şifre belirleme' } },
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
        { path: 'definitions/companies/:id(\\d+)', component: CompanyPage, meta: { title: 'Şirket' } },
        { path: 'definitions/prompts', component: PromptsPage, meta: { title: 'AI istemleri' } },
        { path: 'definitions/users', component: UsersPage, meta: { title: 'Kullanıcılar', admin: true } },
        { path: 'definitions/activity', component: ActivityPage, meta: { title: 'Kayıtlar', admin: true } },
        { path: 'definitions/:kind', component: DefinitionsPage, meta: { title: 'Tanımlar' } },
        { path: ':pathMatch(.*)*', component: NotFoundPage, meta: { title: 'Sayfa bulunamadı' } },
      ],
    },
  ],
})

// Every page except the login and welcome pages needs a signed-in user; Kullanıcılar a yönetici.
router.beforeEach(async (to) => {
  if (to.meta.public) {
    return true
  }
  const user = await loadCurrentUser()
  if (!user) return { path: '/login', query: { returnTo: to.fullPath } }
  return to.meta.admin && user.role !== 'Admin' ? '/definitions' : true
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
