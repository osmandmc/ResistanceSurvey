<template>
  <div ref="modal" class="ui modal">
    <div class="header">
      Şirket Formu
    </div>
    <div class="content">
      <form class="ui form" id="companyForm">
        <!-- Hidden Input -->
        <input type="hidden" v-model="formData.isMain" />
        <div class="field">
          <div class="ui checkbox">
            <input
                type="checkbox"
                v-model="formData.isMain"
            />
            <label>Taşeron</label>
          </div>
        </div>
        <div class="field" v-if="formData.isMain">
          <label for="MainCompanyId">Ana Şirket</label>
          <select
              id="MainCompanyId"
              v-model="formData.mainCompanyId"
          >
            <option value="">--Seçiniz--</option>
            <option v-for="type in companies" :key="type.id" :value="type.id">
              {{ type.name }}
            </option>
          </select>
        </div>
        <!-- Company Name Field -->
        <div class="field">
          <label for="CompanyName">Şirket Adı</label>
          <input
              type="text"
              id="CompanyName"
              v-model="formData.companyName"
              placeholder="Şirket adı giriniz"
          />
        </div>

        <!-- Dropdown Fields -->
        <div class="three fields">
          <!-- TypeId Dropdown -->
          <div class="field">
            <label for="TypeId">Şirketin Niteliği</label>
            <select
                id="TypeId"
                v-model="formData.typeId"
                :disabled="!companyTypes.length"
            >
              <option value="">--Seçiniz--</option>
              <option v-for="type in companyTypes" :key="type.id" :value="type.id">
                {{ type.name }}
              </option>
            </select>
          </div>

          <!-- ScaleId Dropdown -->
          <div class="field">
            <label for="ScaleId">Şirketin Büyüklüğü</label>
            <select
                id="ScaleId"
                v-model="formData.scaleId"
                :disabled="!companyScales.length"
            >
              <option value="">--Seçiniz--</option>
              <option v-for="scale in companyScales" :key="scale.id" :value="scale.id">
                {{ scale.name }}
              </option>
            </select>
          </div>

          <!-- WorklineId Dropdown -->
          <div class="field">
            <label for="WorklineId">Şirketin İş Kolu</label>
            <select
                id="WorklineId"
                v-model="formData.worklineId"
                :disabled="!worklines.length"
            >
              <option value="">--Seçiniz--</option>
              <option v-for="workline in worklines" :key="workline.id" :value="workline.id">
                {{ workline.name }}
              </option>
            </select>
          </div>
        </div>

        <button
            id="btnAddCompany"
            class="ui button positive"
            @click.prevent="addCompany"
        >
          Şirket Ekle
        </button>
      </form>
    </div>
  </div>
</template>

<script>
export default {
  name: "CompanyModal",
  props: {
    isMain: {
      type: Boolean,
      default: false
    },
    // Data passed from the parent, e.g., API dropdown options
    companyTypes: {
      type: Array,
      default: () => [],
    },
    companyScales: {
      type: Array,
      default: () => [],
    },
    worklines: {
      type: Array,
      default: () => [],
    },
    companies: {
      type: Array,
      default: () => [],
    },
  },
  data() {
    return {
      formData: {
        companyName: "",
        typeId: "",
        scaleId: "",
        worklineId: "",
        mainCompanyId: "",
        isMain: this.isMain,
      },
    };
  },
  methods: {
    openModal() {
      console.log(this.formData);
      $(this.$refs.modal).modal("show");
    },
    closeModal() {
      $(this.$refs.modal).modal("hide");
    },
    addMainCompany() {
      console.log("Adding Main Company:", this.formData);
      this.closeModal();
    },
    addCompany() {
      console.log("Adding Company:", this.formData);
      const company = {
        name: this.formData.companyName,
        typeId: this.formData.typeId,
        scaleId: this.formData.scaleId,
        worklineId: this.formData.worklineId,
        isMain: this.formData.isMain
      }
      console.log("Adding Company:", company);
      this.$emit("save-company", company);
      this.closeModal();
    }
  },
};
</script>

<style scoped>
/* Add any scoped styles for the modal if needed */
</style>
