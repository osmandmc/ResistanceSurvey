import { createRouter, createWebHistory } from 'vue-router';
import ListResistance from './components/resistance/ListResistance.vue';
import CreateResistance from './components/resistance/CreateResistance.vue';
import EditResistance from './components/resistance/EditResistance.vue';
import ListNews from './components/news/ListNews.vue';
import NotFound from './components/NotFound.vue';

const routes = [
    {
        path: '/', // Default route for Vue app
        components: {
            LeftSidebar: ListResistance,
        },
    },
    {
        path: '/list', // Relative child path for the list view
        components: {
            LeftSidebar: ListResistance,
        },
    },
    {
        path: '/edit/:id', // Relative path for editing resistance
        components: {
            LeftSidebar: EditResistance,
        },
    },
    {
        path: '/create', // Relative path for create-resistance
        components: {
            LeftSidebar: CreateResistance,
        },
    },
    {
        path: '/:pathMatch(.*)*', // Catch-all route for 404
        name: 'NotFound',
        component: NotFound,
    },
];

const router = createRouter({
    history: createWebHistory('/Resistance/IndexVue'), // Base URL for Vue app
    routes,
});

export default router;