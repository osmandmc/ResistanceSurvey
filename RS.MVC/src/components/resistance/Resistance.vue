<template>
    <!-- Hidden Input for ID -->
    <input type="hidden" name="Id" v-model="this.resistance.id" />

    <div class="field">
      <label for="ResistanceDescription">Kısa Açıklama</label>
      <textarea
          id="ResistanceDescription"
          rows="6"
          v-model="this.resistance.resistanceDescription"
      ></textarea>
    </div>
  
    <div class="field">
      <label for="CategoryId">Vaka Niteliği</label>
      <select v-model="this.resistance.categoryId">
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
                     v-model="this.resistance.resistanceReasonIds"
                     placeholder="Seçiniz" label="name" track-by="id"
                     :preselect-first="true"
                     :options="resistanceReasons"
                     :multiple="true"
                     :close-on-select="false"
                     :clear-on-select="false"
                     :preserve-search="true"
                     :taggable="true"  @tag="addResistanceReason">
        </multiselect>
      </div>

      <!-- Develop Right -->
      <div class="field">
        <label for="DevelopRight">Hak Geliştirme/Hak Savunma Özelliği</label>
        <select v-model="this.resistance.developRight">
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
          <select v-model="this.resistance.companyId">
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
          <button type="button" @click="openCompanyModal" class="ui button">
            <i class="chevron circle up icon"></i>Şirket Ekle
          </button>
        </div>
      </div>
    </div>
    <!-- Is Outsource -->
    <div class="field sixty wide">
      <label for="IsOutsource">Şirket Taşeron mu?</label>
      <select v-model="this.resistance.isOutsource" @change="toggleOutsource">
        <option :value="false">Hayır</option>
        <option :value="true">Evet</option>
      </select>
    </div>

    <!-- Main Company (Conditional) -->
    <div v-if="this.resistance.isOutsource" id="outsource" class="sixty wide field">
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
          <button type="button" @click="openCompanyModal" class="ui button">
            <i class="chevron circle up icon"></i>Ana Şirket Ekle
          </button>
        </div>
      </div>
    </div>

    <!-- Employee Count -->
    <div class="two fields">
      <div class="field">
        <label for="EmployeeCountId">İş Yerindeki İşçi Sayısı</label>
        <select v-model="this.resistance.employeeCountId">
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
            v-model="this.resistance.employeeCount"
        />
      </div>
    </div>
    <div class="field">
      <label for="CorporationIds">Kurumsallık</label>
      <multiselect id="CorporationIds" 
                   v-model="this.resistance.corporationIds"
                   placeholder="Seçiniz" label="name" track-by="id"
                   :preselect-first="true"
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
      <select v-model="this.resistance.resistanceResult" class="ui fluid dropdown">
        <option :value="0">Bilinmiyor</option>
        <option :value="1">Tam Kazanım</option>
        <option :value="2">Yarım Kazanım</option>
        <option :value="3">Sıfır Kazanım</option>
      </select>
    </div>
</template>

<script>
import Multiselect from 'vue-multiselect'

export default {
  name: "Resistance",
  emits: ["openCompanyModal"],
  components: { Multiselect },
  props: {
    // modelValue: Object,
    resistance: {
      type: Object,
      default: () => ({}) 
    },
    companies: {
      type: Array,
      required: true,
    },
    resistanceReasons: {
      type: Array,
      required: true,
    },
    categories: {
      type: Array,
      required: true,
    },
    corporations: {
      type: Array,
      required: true,
    },
    employeeCounts: {
      type: Array,
      required: true,
    },
    formErrors: {
      type:Object,
      default: () => ({})
    }
  },
  methods: {
    toggleOutsource() {
      // Show/hide outsource section
    },
    openCompanyModal() {
      console.log('openCompanyModal');
      this.$emit('openCompanyModal');
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
    },
    customFilter(search, id) {
      console.log(id);
      console.log(search);
      const normalize = (str) =>
          str
              .toLowerCase()
              .normalize("NFD") // Decompose characters
              .replace(/[\u0300-\u036f]/g, "") // Remove diacritics
              .replace(/ı/g, "i")
              .replace(/ğ/g, "g")
              .replace(/ü/g, "u")
              .replace(/ş/g, "s")
              .replace(/ö/g, "o")
              .replace(/ç/g, "c");
      const result = normalize(search);
      console.log(result);
      return result;
    },
  },
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>

<style scoped>
/* Add styles here */
</style>
