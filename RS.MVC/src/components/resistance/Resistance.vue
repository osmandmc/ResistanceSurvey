<template>
    <!-- Hidden Input for ID -->
    <input type="hidden" name="Id" v-model="this.resistance.id" />

    <div class="field">
      <label for="ResistanceDescription">Kısa Açıklama</label>
      <textarea
          id="ResistanceDescription"
          rows="6"
          v-model="this.resistance.resistanceDescription"
          @input="clearError('resistanceDescription')"
      ></textarea>
      <span v-if="formErrors.resistanceDescription" class="field error">
        <label>{{ formErrors.resistanceDescription }}</label>
      </span>
    </div>
  
    <div class="field">
      <label for="CategoryId">Vaka Niteliği</label>
      <select v-model="this.resistance.categoryId"
              @change="clearError('categoryId')">
        <option value="">--Seçiniz--</option>
        <option v-for="category in categories" :key="category.id" :value="category.id">
          {{ category.name }}
        </option>
      </select>
      <span v-if="formErrors.categoryId" class="field error">
        <label>{{ formErrors.categoryId }}</label>
      </span>
    </div>
  <div class="field">
    <label for="IsOutsource">Şirket Taşeron mu?</label>
    <select v-model="this.resistance.isOutsource">
      <option :value="false">Hayır</option>
      <option :value="true">Evet</option>
    </select>
  </div>

  <div class="field">
    <label for="CompanyId">Şirket</label>
    <div class="fields">
      <div class="twelve wide field">
          <multiselect id="single-select-search"
                       v-model="resistance.companyId" 
                       :options="this.companies" 
                       placeholder="Seçiniz" 
                       label="name"
                       track-by="id" 
                       aria-label="seçiniz"
                       @select="clearError('companyId')">
          </multiselect>
          <span v-if="formErrors.companyId" class="field error">
            <label>{{ formErrors.companyId }}</label>
          </span>
        </div>
      <div class="four wide field">
        <button type="button" @click="openCompanyModal(false)" class="ui button">
          <i class="chevron circle up icon"></i>Şirket Ekle
        </button>
      </div>
      </div>
  </div>

  <!-- Main Company (Conditional) -->
  <div v-if="this.resistance.isOutsource" id="outsource" class="field">
    <label for="MainCompanyId">Ana Şirket</label>
    <div class="fields">
      <div class="twelve wide field">
        <multiselect id="single-select-search"
                     v-model="resistance.mainCompanyId"
                     :options="companies"
                     placeholder="Seçiniz"
                     label="name"
                     track-by="id"
                     aria-label="seçiniz"
                     @select="clearError('mainCompanyId')">
        </multiselect>
      </div>
      <div class="four wide field">
        <button type="button" @click="openCompanyModal(true)" class="ui button">
          <i class="chevron circle up icon"></i>Ana Şirket Ekle
        </button>
      </div>
    </div>
  </div>

  <!-- Two Fields -->
    <div class="two fields">
      <!-- Case Reasons -->
      <div class="field">
        <label for="ResistanceReasonIds">Vaka Nedeni</label>
        <multiselect id="ResistanceReasonIds"
                     v-model="this.resistance.resistanceReasonIds"
                     placeholder="Seçiniz" 
                     label="name" 
                     track-by="id"
                     :preselect-first="true"
                     :options="resistanceReasons"
                     :multiple="true"
                     :close-on-select="false"
                     :clear-on-select="false"
                     :preserve-search="true"
                     :taggable="true"  
                     @tag="addResistanceReason"
        >
        </multiselect>
        <span v-if="formErrors.resistanceReasonIds" class="field error">
          <label>{{ formErrors.resistanceReasonIds }}</label>
        </span>

      </div>
      <!-- Develop Right -->
      <div class="field">
        <label for="DevelopRight">Hak Geliştirme/Hak Savunma Özelliği</label>
        <select v-model="this.resistance.developRight"
                @change="clearError('developRight')">
          <option value="">--Seçiniz--</option>
          <option :value="true">Hak Geliştirme</option>
          <option :value="false">Hak Savunma</option>
        </select>
        <span v-if="formErrors.developRight" class="field error">
          <label>{{ formErrors.developRight }}</label>
        </span>
      </div>
    </div>

   
    <!-- Employee Count -->
    <div class="two fields">
     
      <div class="field">
        <label for="EmployeeCount">İş Yerindeki İşçi Sayısı (Tam)</label>
        <input
            type="number"
            id="EmployeeCount"
            v-model="this.resistance.employeeCount"
        />
      </div>
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
      </div>
    </div>
    <div class="field">
      <label for="CorporationIds">Kurumsallık</label>
      <multiselect 
          v-model="this.resistance.corporationIds"
          placeholder="Seçiniz" label="name" track-by="id"
          :preselect-first="true"
          :options="corporations"
          :multiple="true" 
          :close-on-select="false" 
          :clear-on-select="false"
          :preserve-search="true" 
          :taggable="true" @tag="addCorporation"
          @select="clearError('corporationIds')"
      >
      </multiselect>
      <span v-if="formErrors.corporationIds" class="field error">
        <label>{{ formErrors.corporationIds }}</label>
      </span>
    </div>
  <div class="field" v-show="showTradeUnionAuthority">
    <label for="TradeUnionAuthorityId">Sendikanın Yetki Durumu</label>
    <select v-model="this.resistance.tradeUnionAuthorityId">
      <option value="">--Seçiniz--</option>
      <option
          v-for="ta in this.tradeUnionAuthorities"
          :key="ta.id"
          :value="ta.id"
      >
        {{ ta.name }}
      </option>
    </select>
  </div>
  <div class="field">
    <label for="TradeUnionId">Tepki Gösterilen Sendika</label>
    <select v-model="this.resistance.tradeUnionId">
      <option value="">--Seçiniz--</option>
      <option
          v-for="ta in this.tradeUnions"
          :key="ta.id"
          :value="ta.id"
      >
        {{ ta.name }}
      </option>
    </select>
  </div>
  <div class="two fields">
    <div class="field">
      <label for="EmploymentTypeIds">İstihdam Türü</label>
      <multiselect id="CorporationIds"
                   v-model="this.resistance.employmentTypeIds"
                   placeholder="Seçiniz" label="name" track-by="id"
                   :preselect-first="true"
                   :options="employmentTypes"
                   :multiple="true"
                   :close-on-select="false"
                   :clear-on-select="false"
                   :preserve-search="true"
                   :taggable="true"
                   @select="clearError('employmentTypeIds')">
      </multiselect>
      <div v-if="formErrors.employmentTypeIds" class="field error">
        <label>{{ formErrors.employmentTypeIds }}</label>
      </div>
    </div>
  </div>
  <div class="field">
    <label for="FiredEmployeeCountByProtesto">Mücadele Ettiği için İşten Atılan İşçi Sayısı</label>
    <input type="number" v-model="this.resistance.firedEmployeeCountByProtesto">
  </div>
    <!-- Other Fields -->
    <div class="field">
      <label for="ResistanceResult">Sonuç</label>
      <select v-model="this.resistance.resistanceResult" 
              class="ui fluid dropdown"
              @change="clearError('resistanceResult')"
      >
        <option :value="0">Bilinmiyor</option>
        <option :value="1">Tam Kazanım</option>
        <option :value="2">Yarım Kazanım</option>
        <option :value="3">Sıfır Kazanım</option>
      </select>
      <div v-if="formErrors.resistanceResult" class="field error">
        <label>{{ formErrors.resistanceResult }}</label>
      </div>
    </div>
</template>

<script>
import Multiselect from 'vue-multiselect'

export default {
  name: "Resistance",
  emits: ["openCompanyModal", "onInputChanged"],
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
    tradeUnionAuthorities: {
      type: Array,
      required: true,
    },
    tradeUnions: {
      type: Array,
      required: true,
    },
    employmentTypes: {
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
    clearError(field) {
      // Clear the error message for the specified field
      console.log(field);
      this.$emit('onInputChanged', field);
    },
    openCompanyModal(isMain) {
      console.log(isMain);
      this.$emit('openCompanyModal', isMain);
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
  },
  computed: {
    showTradeUnionAuthority(){
      return this.resistance?.corporationIds?.some(s=>s.CorporationTypeId === 1);
    }
  }
};
</script>
<style src="vue-multiselect/dist/vue-multiselect.min.css"></style>

<style scoped>
/* Add styles here */
</style>
