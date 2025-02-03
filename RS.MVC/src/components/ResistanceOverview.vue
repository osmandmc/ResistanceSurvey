<template>
  <div class="ui two column divided grid">
    <div class="row">
      <!-- Resistance Section -->
      <div class="column">
        <router-view name="LeftSidebar" /> <!-- This renders the left components based on route -->
      </div>

      <!-- News Section -->
      <div class="column">
        <div class="edit-section">
        </div>
        <router-view name="RightSidebar" /> <!-- This renders the right components (e.g., ListNews) -->
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

export default {
  name: 'ResistanceOverview',
  components: {
    ListResistance,
    ListNews,
    CreateResistance,
    EditResistance,
    Modal,
  },
  mounted() {
    
  }
};
</script>

<style scoped>
/* Add your styles here */
</style>
