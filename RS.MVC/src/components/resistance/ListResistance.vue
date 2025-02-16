<template>
  <div>
    <div v-if="isLoading" class="ui dimmer active">
        <div class="ui text loader">İşlem Yapılıyor...</div>
    </div>

    <div v-else>
      <h4 class="ui dividing header">
        <router-link to="/create" class="item"><i class="plus icon"></i></router-link>
        Vakalar
      </h4>
      <a href="#" @click="reloadList" style="position:absolute; top:0; right:1em">
        <i class="redo alternate icon"></i>
      </a>

      <div class="ui attached secondary segment resistanceFilter">
        <div class="ui accordion field">
          <div class="title">
            <i class="icon dropdown"></i>
            Filtreler
          </div>
          <div class="content ui form">
            <div class="field">
              <label class="transition visible">Şirketler</label>
              <select v-model="filter.companyId" class="ui fluid search selection dropdown">
                <option value="">--Seçiniz--</option>
                <option v-for="company in companies" :key="company.id" :value="company.id">{{ company.name }}</option>
              </select>
            </div>
            <div class="field">
              <label class="transition visible">Ana Şirket</label>
              <select v-model="filter.mainCompanyId" class="ui fluid search selection dropdown">
                <option value="">--Seçiniz--</option>
                <option v-for="company in companies" :key="company.id" :value="company.id">{{ company.name }}</option>
              </select>
            </div>
            <div class="field">
              <label class="transition visible">Vaka Niteliği</label>
              <select v-model="filter.categoryId" class="ui fluid search selection dropdown">
                <option value="">--Seçiniz--</option>
                <option v-for="category in categories" :key="category.id" :value="category.id">{{
                    category.name
                  }}
                </option>
              </select>
            </div>
            <div class="fields">
              <div class="field">
                <label class="transition visible">Eylem Yıl</label>
                <select v-model="filter.yearId" class="ui fluid selection dropdown">
                  <option value="">--Seçiniz--</option>
                  <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
                </select>
              </div>
              <div class="field">
                <label class="transition visible">Eylem Ay</label>
                <select v-model="filter.monthId" class="ui fluid search selection dropdown">
                  <option value="">--Seçiniz--</option>
                  <option v-for="(month, index) in months" :key="index" :value="index + 1">{{ month }}</option>
                </select>
              </div>
            </div>
            <div class="field">
              <label class="transition visible">Kişisel Notlar</label>
              <select v-model="filter.personalNote" class="ui fluid search selection dropdown">
                <option value="">--Seçiniz--</option>
                <option value="false">Yok</option>
                <option value="true">Var</option>
              </select>
            </div>
            <div class="field">
              <button @click="applyFilter" class="ui button">Filtrele</button>
              <button @click="clearFilter" class="ui basic button clear">Temizle</button>
              <button @click="exportData" class="ui green basic button clear">Dışa Aktar</button>
            </div>
          </div>
        </div>
      </div>

      <table class="ui celled selectable striped table">
        <tbody>
        <tr v-for="item in results" :key="item.id">
          <td v-if="!isEditing">
            <router-link :to="`/edit/${item.id}`">{{ item.categoryName }}</router-link>
          </td>
          <td>
            <router-link :to="`/edit/${item.id}`">{{ item.companyName }}</router-link>
          </td>
          <td>{{ formatDate(item.startDate) }}</td>
        </tr>
        </tbody>
        <tfoot>
        <tr>
          <th :colspan="isEditing ? 1 : 2">
            <p>{{ rowCount }} kayıttan {{ from }} - {{ to }} arasındakiler görüntüleniyor.</p>
          </th>
          <th>
            <div class="field">
              <select v-model="filter.pageNumber" class="ui compact small dropdown" 
                      @change="reloadList">
                <option v-for="i in pageCount" :key="i" :value="i">{{ i }}</option>
              </select>
            </div>
          </th>
        </tr>
        </tfoot>
      </table>
    </div>
  </div>
</template>

<script>
import {fetchWithToken} from "../../fetchWrapper";

export default {
  name: "ListResistance",
  data() {
    return {
      isLoading: true,
      filter: {
        companyId: "",
        mainCompanyId: "",
        categoryId: "",
        yearId: "",
        monthId: "",
        personalNote: "",
        pageNumber: 1,
        pageSize: 10, // Define a default page size
      },
      rowCount: 0,
      pageSize: 10,
      companies: [],
      categories: [],
      results: [],
      years: Array.from({length: new Date().getFullYear() - 2017}, (_, i) => 2018 + i),
      months: [
        "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
        "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
      ],
    };
  },
  computed: {
    filteredResults() {
      return this.results.filter(item => {
        return (
            (!this.filter.companyId || item.companyId === this.filter.companyId) &&
            (!this.filter.mainCompanyId || item.mainCompanyId === this.filter.mainCompanyId) &&
            (!this.filter.categoryId || item.categoryId === this.filter.categoryId) &&
            (!this.filter.yearId || new Date(item.startDate).getFullYear() === this.filter.yearId) &&
            (!this.filter.monthId || new Date(item.startDate).getMonth() + 1 === this.filter.monthId) &&
            (!this.filter.personalNote || item.personalNote === JSON.parse(this.filter.personalNote))
        );
      });
    },
    from() {
      return Math.min((this.filter.pageNumber - 1) * this.filter.pageSize + 1, this.rowCount);
    },
    to() {
      return Math.min(this.filter.pageNumber * this.filter.pageSize, this.rowCount);
    },
    pageCount() {
      return Math.ceil(this.rowCount / this.filter.pageSize);
    },
    isEditing() {
      return this.$route.path.includes('/edit/');
    },
  },
  methods: {
    applyFilter() {
      this.filter.pageNumber = 1;
      this.reloadList();
    },
    clearFilter() {
      this.filter = {
        companyId: "",
        mainCompanyId: "",
        categoryId: "",
        yearId: "",
        monthId: "",
        personalNote: "",
        pageNumber: 1,
        pageSize: 10,
      };
      this.reloadList();
    },
    reloadList() {
      this.fetchResults();
    },
    fetchResults() {
      this.isLoading = true;
      const queryParams = new URLSearchParams(this.filter).toString();
      fetchWithToken(`/resistance/list?${queryParams}`)
      .then(response => response.json())
      .then(data => {
        console.log(data);
        this.results = data.results || [];
        this.filter = data.filter;
        this.rowCount = data.rowCount;
      })
      .catch(err => console.error("Error fetching results:", err))
      .finally(() => (this.isLoading = false));
    },
    exportData() {
      console.log("Exporting data...");
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString();
    },
    nextPage() {
      if (this.filter.pageNumber < this.pageCount) {
        this.filter.pageNumber++;
        this.reloadList(); // Fetch data for the next page
      }
    },
    previousPage() {
      if (this.filter.pageNumber > 1) {
        this.filter.pageNumber--;
        this.reloadList(); // Fetch data for the previous page
      }
    },
    initializeSemanticUI() {
      // Reinitialize Semantic UI components
      this.$nextTick(() => {
        $('.ui.dropdown').dropdown({
          clearable: true,
          fullTextSearch: 'exact',
        });
        $('.ui.accordion').accordion();
      });
    },
  },
  mounted() {
    this.reloadList();
    fetchWithToken("/company/list")
        .then(response => response.json())
        .then(data => (this.companies = data));

    fetchWithToken("/lookup/categories")
        .then(response => response.json())
        .then(data => (this.categories = data));
    this.initializeSemanticUI();
  },
  updated() {
    // Reinitialize Semantic UI components after the DOM updates
    this.initializeSemanticUI();
  },
};
</script>

<style scoped>
.ui.column.dimmer.active {
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.8);
  z-index: 10;
}
</style>
