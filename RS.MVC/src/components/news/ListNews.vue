<template>
    <!-- Header -->
    <h4 class="ui dividing header">Haberler</h4>

    <!-- Filter Section -->
    <div class="ui attached secondary segment">
      <div class="ui small filter form">
        <div class="fields">
          <!-- Year Dropdown -->
          <div class="four wide field">
            <select
                v-model="selectedYear"
                class="ui fluid search selection dropdown"
            >
              <option v-for="year in years" :key="year" :value="year">
                {{ year }}
              </option>
            </select>
          </div>

          <!-- Month Dropdown -->
          <div class="five wide field">
            <select
                  v-model="selectedMonth"
                class="ui fluid search selection dropdown"
            >
              <option value="">--Seçiniz--</option>
              <option v-for="(month, index) in months" :key="index" :value="index + 1">
                {{ month }}
              </option>
            </select>
          </div>

          <!-- Filter Button -->
          <div class="four wide field">
            <button @click="fetchNews" class="ui left labeled icon button">
              <i class="filter icon"></i>Filtrele
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- News Section -->
    <div style="overflow-y: scroll; height:1000px;">
    <div v-for="item in filteredNews" :key="item.id" class="item" :data-id="item.id">
      <div class="content">
        <!-- Header and Link -->
        <a
            v-if="item.added"
            href="#"
            class="a"
            style="color:red"
        >
          {{ item.header }}
        </a>
        <a
            v-else
            href="#"
            :data-id="item.id"
            class="a"
        >
          {{ item.header }}
        </a>

        <!-- External Link -->
        <a :href="item.link" target="_blank">
          <i
              class="arrow alternate circle right outline icon"
              style="color: red"
              v-if="item.added"
          ></i>
          <i
              class="arrow alternate circle right outline icon"
              v-else
          ></i>
        </a>

        <!-- Meta Section (Date) -->
        <div class="meta">
          <span class="date">{{ formatDate(item.date) }}</span>
          <button
              @click="linkNews(item.id)"
              class="ui icon button green basic mini copyLink"
          >
            <i class="paperclip icon"></i>
          </button>
        </div>

        <!-- Description (Hidden by Default) -->
        <div
            class="description"
            v-show="false"
        ></div>
      </div>
    </div>
  </div>
</template>

<script>
import {fetchWithToken} from "../../fetchWrapper";

export default {
  name: "ListNews",
  emits: ["linkNews"],
  data() {
    return {
      selectedYear: new Date().getFullYear(), // Default to current year
      selectedMonth: "", // No month selected by default
      years: Array.from({ length: new Date().getFullYear() - 2018 + 1 }, (_, i) => 2019 + i), // Generate years dynamically
      months: [
        "Ocak",
        "Şubat",
        "Mart",
        "Nisan",
        "Mayıs",
        "Haziran",
        "Temmuz",
        "Ağustos",
        "Eylül",
        "Ekim",
        "Kasım",
        "Aralık",
      ],
      news: [], // Array to hold news items
    };
  },
  computed: {
    filteredNews() {
      return this.news;
    },
  },
  mounted() {
    this.fetchNews();
  },
  methods: {
    fetchNews() {
      console.log(this.selectedYear);
      // Simulate an API call to fetch news data
      fetchWithToken(`/resistance/news?year=${this.selectedYear}&month=${this.selectedMonth}`)
          .then(response => response.json())
          .then(data => (this.news = data));
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date to a readable string
    },
    linkNews(id){
      const newsItem = this.filteredNews.find(news => news.id === id);
      this.$emit('linkNews', newsItem);
    },
  },
};
</script>

<style scoped>
.item {
  padding: 1rem;
  border-bottom: 1px solid #f1f1f1;
}
.meta {
  margin-top: 10px;
  display: flex;
  align-items: center;
}
.date {
  margin-right: 10px;
}
</style>
