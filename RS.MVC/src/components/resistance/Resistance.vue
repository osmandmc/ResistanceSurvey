<template>
    <!-- Header -->
    <h4 class="ui dividing header">
      <router-link to="/list" class="item"><i class="angle double left icon"></i></router-link>
      Vaka
    </h4>

    <!-- Hidden Input for ID -->
    <input type="hidden" name="Id" v-model="this.resistanceData.id" />

    <!-- Short Description -->
    <div class="field">
      <label for="ResistanceDescription">Kısa Açıklama</label>
      <textarea
          id="ResistanceDescription"
          rows="6"
          v-model="this.resistanceData.resistanceDescription"
      ></textarea>
    </div>

    <!-- Case Category -->
    <div class="field">
      <label for="CategoryId">Vaka Niteliği</label>
      <select v-model="this.resistanceData.categoryId">
        <option value="">--Seçiniz--</option>
        <option v-for="category in categories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </select>
      <span v-if="formErrors.categoryId" class="text-danger">
        {{ formErrors.categoryId }}
      </span>
    </div>

    <!-- Two Fields -->
    <div class="two fields">
      <!-- Case Reasons -->
      <div class="field">
        <label for="ResistanceReasonIds">Vaka Nedeni</label>
        <multiselect id="ResistanceReasonIds"
                     v-model="this.resistanceData.resistanceReasonIds"
                     placeholder="Seçiniz" label="name" track-by="id"
                     :preselect-first="true"
                     :options="resistanceReasons"
                     :multiple="true"
                     :close-on-select="false"
                     :clear-on-select="false"
                     :preserve-search="true"
                     :taggable="true" @tag="addResistanceReason">
        </multiselect>
      </div>

      <!-- Develop Right -->
      <div class="field">
        <label for="DevelopRight">Hak Geliştirme/Hak Savunma Özelliği</label>
        <select v-model="this.resistanceData.developRight">
          <option value="">--Seçiniz--</option>
          <option :value="true">Hak Geliştirme</option>
          <option :value="false">Hak Savunma</option>
        </select>
        <span v-if="formErrors.developRight" class="text-danger">
          {{ formErrors.developRight }}
        </span>
      </div>
    </div>

    <div class="sixty wide field">
      <label for="CompanyId">Şirket</label>
      <div class="two fields">
        <div class="field">
          <select v-model="this.resistanceData.companyId">
            <option value="">--Seçiniz--</option>
            <option
                v-for="company in companies"
                :key="company.id"
                :value="company.id"
            >
              {{ company.name }}
            </option>
          </select>
        </div>
        <div class="field">
          <button type="button" @click="openCompanyModal(true)" class="ui button">
            <i class="chevron circle up icon"></i>Şirket Ekle
          </button>
        </div>
      </div>
    </div>
    <!-- Is Outsource -->
    <div class="field sixty wide">
      <label for="IsOutsource">Şirket Taşeron mu?</label>
      <select v-model="this.resistanceData.isOutsource" @change="toggleOutsource">
        <option :value="false">Hayır</option>
        <option :value="true">Evet</option>
      </select>
    </div>

    <!-- Main Company (Conditional) -->
    <div v-if="this.resistanceData.isOutsource" id="outsource" class="sixty wide field">
      <label for="MainCompanyId">Ana Şirket</label>
      <div class="two fields">
        <div class="field">
          <select v-model="resistance.mainCompanyId">
            <option value="">--Seçiniz--</option>
            <option
                v-for="company in companies"
                :key="company.id"
                :value="company.id"
            >
              {{ company.name }}
            </option>
          </select>
        </div>
        <div class="field">
          <button type="button" @click="openCompanyModal(true)" class="ui button">
            <i class="chevron circle up icon"></i>Ana Şirket Ekle
          </button>
        </div>
      </div>
    </div>

    <!-- Employee Count -->
    <div class="two fields">
      <div class="field">
        <label for="EmployeeCountId">İş Yerindeki İşçi Sayısı</label>
        <select v-model="this.resistanceData.employeeCountId">
          <option value="">--Seçiniz--</option>
          <option
              v-for="count in employeeCounts"
              :key="count.id"
              :value="count.id"
          >
            {{ count.name }}
          </option>
        </select>
        <span v-if="formErrors.employeeCountId" class="text-danger">
          {{ formErrors.employeeCountId }}
        </span>
      </div>
      <div class="field">
        <label for="EmployeeCount">İş Yerindeki İşçi Sayısı (Tam)</label>
        <input
            type="text"
            id="EmployeeCount"
            v-model="this.resistanceData.employeeCount"
        />
      </div>
    </div>
    <div class="field">
      <label for="CorporationIds">Kurumsallık</label>
      <multiselect id="CorporationIds" 
                   v-model="this.resistanceData.corporationIds"
                   placeholder="Seçiniz" label="name" track-by="id" :preselect-first="true"
                   :options="corporations"
                   :multiple="true" 
                   :close-on-select="false" 
                   :clear-on-select="false"
                   :preserve-search="true" 
                   :taggable="true" @tag="addCorporation">
      </multiselect>
    </div>
    <!-- Other Fields -->
    <div class="field">
      <label for="ResistanceResult">Sonuç</label>
      <select v-model="this.resistanceData.resistanceResult" class="ui fluid dropdown">
        <option :value="0">Bilinmiyor</option>
        <option :value="1">Tam Kazanım</option>
        <option :value="2">Yarım Kazanım</option>
        <option :value="3">Sıfır Kazanım</option>
      </select>
    </div>

    <!-- Protestos -->
    <h3 class="ui dividing header">Eylemler</h3>
    <button
        type="button"
        class="ui right labeled small green icon button"
        @click="addProtesto"
    >
      Eylem Ekle<i class="plus icon"></i>
    </button>
  
  
</template>

<script>
import Multiselect from 'vue-multiselect'
import {fetchWithToken} from "../../fetchWrapper";

export default {
  name: "Resistance",
  components: { Multiselect },
  data() {
    return {
      resistanceData: this.resistance,
      categories: [],
      resistanceReasons: [],
      companies: [],
      corporations: [],
      employeeCounts: []
    };
  },
  props: {
    resistance: {
      type: Object,
      default: () => ({}) 
    },
    formErrors: {
      type:Object,
      default: () => ({})
    }
  },
  mounted() {
    fetchWithToken("/company/list")
        .then(response => response.json())
        .then(data => (this.companies = data));

    fetchWithToken("/resistance/categories")
        .then(response => response.json())
        .then(data => (this.categories = data));

    fetchWithToken("/lookup/resistancereasons")
        .then(response => response.json())
        .then(data => (this.resistanceReasons = data));

    fetchWithToken("/lookup/employeeCounts")
        .then(response => response.json())
        .then(data => (this.employeeCounts = data));
    
    fetchWithToken("/corporation/list")
        .then(response => response.json())
        .then(data => (this.corporations = data));
    
  },
  methods: {
    toggleOutsource() {
      // Show/hide outsource section
    },
    addResistanceReason (newTag) {
      const tag = {
        name: newTag,
        id: -1
      }
      this.resistanceReasons.push(tag)
      this.resistance.resistanceReasonIds.push(tag)
    },
    addEmploymentType(newTag) {
      const tag = {
        name: newTag,
        id: -1
      }
      this.employmentTypes.push(tag)
      this.resistance.employmentTypeIds.push(tag)
    },
    addCorporation(newTag) {
      const tag = {
        name: newTag,
        id: -1
      }
      this.corporations.push(newTag);
      this.resistance.corporationIds.push(tag)
    }
  },
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>

<style scoped>
/* Add styles here */
</style>
