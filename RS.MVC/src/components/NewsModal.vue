<template>
  <div ref="modal" class="ui modal">
    <list-news></list-news>
  </div>
</template>

<script>
import ListNews from './news/ListNews';
export default {
  name: "NewsModal",
  components: { ListNews },
  methods: {
    openModal() {
      console.log(this.formData);
      $(this.$refs.modal).modal("show");
    },
    closeModal() {
      $(this.$refs.modal).modal("hide");
    },
    linkNews(news) {
      console.log(news);
    },
  },
};
</script>

<style scoped>
/* Add any scoped styles for the modal if needed */
</style>
