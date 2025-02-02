<template>
  <form @submit.prevent="submitForm" class="ui form" id="resistanceForm">
    <h4 class="ui dividing header">
      <router-link to="/list" class="item"><i class="angle double left icon"></i></router-link>
      Vakalar
    </h4>

    <div class="field">
      <label for="ResistanceDescription">Kısa Açıklama</label>
      <textarea v-model="form.ResistanceDescription" id="ResistanceDescription" rows="3"></textarea>
    </div>

    <div class="field">
      <label for="CategoryId">Vaka Niteliği</label>
      <select v-model="form.CategoryId" class="ui fluid search selection dropdown">
        <option value="">--Seçiniz--</option>
        <option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option>
      </select>
    </div>

    <div class="fields">
      <div class="six wide field">
        <label for="IsOutsource">Şirket Taşeron mu?</label>
        <select v-model="form.IsOutsource" class="ui dropdown">
          <option value="false">Hayır</option>
          <option value="true">Evet</option>
        </select>
      </div>
      <div class="ten wide field">
        <label for="CompanyId">Şirket</label>
        <div class="two fields">
          <div class="field">
            <select v-model="form.CompanyId" @change="checkExisiting" class="ui fluid search selection dropdown">
              <option value="">--Seçiniz--</option>
              <option v-for="company in companies" :key="company.id" :value="company.id">{{ company.name }}</option>
            </select>
          </div>
          <div class="field">
            <button type="button" @click="openModal(false)" class="ui button">Yeni Şirket Ekle</button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="form.IsOutsource" class="field">
      <label for="MainCompanyId">Ana Şirket</label>
      <div class="two fields">
        <div class="field">
          <select v-model="form.MainCompanyId" class="ui fluid search selection dropdown">
            <option value="">--Seçiniz--</option>
            <option v-for="company in companies" :key="company.id" :value="company.id">{{ company.name }}</option>
          </select>
        </div>
        <div class="field">
          <button type="button" @click="openModal(true)" class="ui button">Ana Şirket Ekle</button>
        </div>
      </div>
    </div>

    <div class="field">
      <label for="ResistanceReasonIds">Vaka Nedeni</label>
      <select v-model="form.ResistanceReasonIds" multiple class="ui fluid search dropdown">
        <option v-for="reason in resistanceReasons" :key="reason.id" :value="reason.id">{{ reason.name }}</option>
      </select>
    </div>

    <div class="field">
      <label for="DevelopRight">Hak Geliştirme/Hak Savunma Özelliği</label>
      <select v-model="form.DevelopRight" class="ui dropdown">
        <option value="">--Seçiniz--</option>
        <option value="true">Hak Geliştirme</option>
        <option value="false">Hak Savunma</option>
      </select>
    </div>

    <!-- More Fields (Repeat for other fields like EmployeeCount, CorporationIds, etc.) -->

    <div class="field">
      <button type="submit" class="ui ok green button">KAYDET</button>
    </div>

    <div class="ui modal" id="addCompanyModal">
      <i class="close icon"></i>
      <div class="header">Yeni Şirket Ekle</div>
      <div class="content" id="modal-content">
        <!-- Modal Content Here -->
      </div>
    </div>

    <div class="ui modal" id="addOutsourceCompanyModal">
      <i class="close icon"></i>
      <div class="header">Yeni Taşeron Şirket Ekle</div>
      <div class="content" id="modalOutsource-content">
        <!-- Modal Content Here -->
      </div>
    </div>
  </form>
</template>

<script>
export default {
  data() {
    return {
      form: {
        ResistanceDescription: '',
        CategoryId: '',
        IsOutsource: false,
        CompanyId: '',
        MainCompanyId: '',
        ResistanceReasonIds: [],
        DevelopRight: '',
        EmployeeCount: '',
        EmployeeCountId: '',
        CorporationIds: [],
        TradeUnionAuthorityId: '',
        TradeUnionId: '',
        EmploymentTypeIds: [],
        GenderId: '',
        FiredEmployeeCountByProtesto: '',
        ResistanceResult: 0,
        ProtestoTypeIds: [],
        StrikeDuration: '',
        SimpleProtestoDescription: '',
        ProtestoStartDate: '',
        ProtestoEndDate: '',
        ProtestoPlaceIds: [],
        EmployeeCountInProtesto: '',
        EmployeeCountInProtestoId: '',
        InterventionTypeIds: [],
        CustodyCount: '',
        Note: ''
      },
      categories: [],
      companies: [],
      resistanceReasons: [],
      // Add other required arrays (e.g., ProtestoTypes, EmployeeCount, etc.)
    };
  },
  mounted() {
    // Fetch the lookup data (categories, companies, resistance reasons, etc.)
    this.fetchLookupData();
  },
  methods: {
    async fetchLookupData() {
      try {
        const [categories, companies, resistanceReasons] = await Promise.all([
          this.fetchCategories(),
          this.fetchCompanies(),
          this.fetchResistanceReasons(),
        ]);
        this.categories = categories;
        this.companies = companies;
        this.resistanceReasons = resistanceReasons;
      } catch (error) {
        console.error('Error fetching lookup data', error);
      }
    },
    async fetchCategories() {
      const response = await fetch('/api/lookup/categories');
      return await response.json();
    },
    async fetchCompanies() {
      const response = await fetch('/api/lookup/companies');
      return await response.json();
    },
    async fetchResistanceReasons() {
      const response = await fetch('/api/lookup/resistance-reasons');
      return await response.json();
    },
    openModal(isOutsource) {
      const modalId = isOutsource ? '#addOutsourceCompanyModal' : '#addCompanyModal';
      const modal = document.querySelector(modalId);
      $(modal).modal('show');
    },
    checkExisiting() {
      const companyId = this.form.CompanyId;
      const categoryId = this.form.CategoryId;
      if (companyId && categoryId) {
        this.existingResistance(companyId, categoryId);
      }
    },
    async existingResistance(companyId, categoryId) {
      try {
        const response = await fetch(`/Resistance/ExistingResistance?companyId=${companyId}&categoryId=${categoryId}`);
        const data = await response.json();
        if (data) {
          alert('Mevcut');
        }
      } catch (error) {
        console.error('Error checking existing resistance', error);
      }
    },
    submitForm() {
      // Perform form validation and submission
      console.log(this.form);
      // Add form submission logic here
    },
    goBack() {
      this.$router.push({ name: 'list' }); // Navigate back to list view (assuming you have a router set up)
    }
  }
};
</script>

<style scoped>
/* Add custom styles if needed */
</style>
