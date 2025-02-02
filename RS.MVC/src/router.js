import { createRouter, createWebHistory } from 'vue-router';
import ResistanceOverview from './components/ResistanceOverview.vue';
import ListResistance from './components/resistance/list-resistance.vue';
import CreateResistance from './components/resistance/create-resistance.vue';
import EditResistance from './components/resistance/edit-resistance.vue';
import ListNews from './components/news/list-news.vue';
import NotFound from './components/NotFound.vue';
const routes = [
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: NotFound,
    },
    {
        path: '/', // Default child for list-resistance and list-news
        components: {
            // short for LeftSidebar: LeftSidebar
            LeftSidebar: ListResistance,
            // they match the `name` attribute on `<router-view>`
            RightSidebar: ListNews,
        },
    },
    {
        path: '/list', // Relative child path for the list view
        components: {
            LeftSidebar: ListResistance,
            RightSidebar: ListNews,
        },
    },
    {
        path: '/edit/:id', // Relative path for editing resistance
        components: {
            LeftSidebar: ListResistance,
            RightSidebar: EditResistance,
        },
    },
    {
        path: '/create', // Relative path for create-resistance
        components: {
            LeftSidebar: CreateResistance,
            RightSidebar: ListNews,
        },
    }
];

const router = createRouter({
    history: createWebHistory('/Resistance/IndexVue'),
    routes,
});

export default router;
