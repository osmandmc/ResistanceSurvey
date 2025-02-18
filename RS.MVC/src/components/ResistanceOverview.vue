<template>
  <div class="ui celled divided grid">
    <div class="row">
      <div class="nine wide column">
        <router-view name="LeftSidebar"></router-view>
<!--        <ListResistance />-->
<!--        <ListResistance :isEditing="isEditing" />-->
      </div>

      <!-- Middle Column: Appears when editing -->
<!--      <div class="six wide column" v-if="isEditing">-->
<!--        <EditResistance :id="$route.params.id" />-->
<!--      </div>-->
     

      <!-- News Section -->
      <div class="seven wide column">
        <ListNews @linkNews="handlelinkNews" />
      </div>
    </div>
  </div>
</template>

<script>
import ListResistance from './resistance/ListResistance.vue';
import ListNews from './news/ListNews.vue';
import CreateResistance from './resistance/CreateResistance.vue';
import EditResistance from './resistance/EditResistance.vue';
import Modal from './CompanyModal.vue';
import { provide, reactive } from 'vue';

export default {
  name: 'ResistanceOverview',
  data() {
    return {
      addedNews: {}
    }
  },
  components: {
    ListResistance,
    ListNews,
    CreateResistance,
    EditResistance,
    Modal,
  },
  setup() {
    const addedNews = reactive({ news: {} });
    const handlelinkNews = (news) => {
      addedNews.news = news; // Update the reactive object
    };

    provide('addedNews', addedNews); // Make it reactive

    return { handlelinkNews };
  },
  computed: {
    isEditing() {
      return this.$route.path.includes('/edit/');
    },
    leftColumnClass() {
      return this.isEditing ?  "four wide column" : "eight wide column";
    },
    rightColumnClass() {
      return this.isEditing ? "six wide column" : "eight wide column";
    }
  },
  mounted() {
    
  },
  // methods: {
  //   handlelinkNews(news){
  //     console.log('main', news);
  //     this.addedNews = news;
  //   }
  // }
};
</script>

<style scoped>
/* Add your styles here */
</style>
