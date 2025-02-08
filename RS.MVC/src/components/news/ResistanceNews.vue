<template>
      <h3 class="ui dividing header">Haberler</h3>
    <div class="ui divided items" id="resistanceNews">
      <div v-for="(news, index) in this.news" :key="news.id" class="item" :data-id="news.id">
        <div class="content">
          <a href="#" :data-id="news.id" :data-open="0" @click.prevent="toggleDescription(news.id)">
            {{ news.header }}
          </a>
          <a :href="news.link" target="_blank">
            <i class="arrow alternate circle right outline icon"></i>
          </a>
          <div class="meta">
            <span class="date">{{ formatDate(news.date) }}</span>
            <button
                type="button"
                class="ui icon button red basic mini btnRemove"
                :data-id="news.id"
                @click="removeNews(news.id)"
            >
              <i class="trash icon"></i>
            </button>
          </div>
          <div class="description" :data-id="news.id" v-show="news.isOpen">
            {{ news.description }}
          </div>
          <input type="hidden" :name="`ResistanceNewsIds[${index}].Id`" :value="news.id">
          <input type="hidden" :name="`ResistanceNewsIds[${index}].IsDeleted`" :value="news.isDeleted ? 1 : 0">
        </div>
      </div>
    </div>
  </template>

<script>
export default {
  name: 'ResistanceNews',
  emits: ["openNewsModal", "removeNews"],
  props: {
    news: {
      type: Array,
      default() {() => []}
    }
  },
  data() {
    return {
      resistanceNews: {...this.news },
    };
  },
  mounted() {
    console.log('ResistanceNews', this.news);
  },
  methods: {
    openNewsModal(){
      console.log('OpenNewsModal emit');
      this.$emit('openNewsModel');
    },
    toggleDescription(id) {
      const newsItem = this.resistanceNews.find(news => news.id === id);
      if (newsItem) {
        newsItem.isOpen = !newsItem.isOpen;
      }
    },
    linkNews(id){
      const newsItem = this.resistanceNews.find(news => news.id === id);
      this.$emit('linkNews', newsItem);
    },
    removeNews(id) {
      console.log('RemoveNews', id);

      this.$emit("removeNews", id)
    },
    formatDate(date) {
      const d = new Date(date);
      return d.toLocaleDateString();
    }
  }
};
</script>

<style scoped>
.ui.dividing.header {
  margin-bottom: 1rem;
}
.ui.divided.items .item {
  padding: 1rem 0;
  border-bottom: 1px solid #ddd;
}
.ui.icon.button {
  margin-left: 0.5rem;
}
</style>
