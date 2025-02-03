<template>
    <!-- Protesto Types -->
    <div class="field">
      <label for="ProtestoTypeIds">Eylem Türleri</label>
      <multiselect
          id="ProtestoTypeIds"
          v-model="this.protesto.protestoTypeIds"
          :options="protestoTypeOptions"
          :multiple="true"
          :close-on-select="false"
          :clear-on-select="false"
          :preserve-search="true"
          placeholder="Seçiniz"
          label="name"
          track-by="id"
      ></multiselect>
    </div>

    <!-- Start and End Dates -->
    <div class="two fields">
      <div class="field">
        <label for="ProtestoStartDate">Başlangıç Tarihi</label>
        <VueDatePicker v-model="this.protesto.protestoStartDate" 
                       text-input 
                       locale="tr-TR"
                       :format-locale="tr" 
                       format="dd/MM/yyyy">
        </VueDatePicker>
      </div>
      <div class="field">
        <label for="ProtestoEndDate">Bitiş Tarihi</label>
        <VueDatePicker v-model="this.protesto.protestoEndDate" 
                       text-input
                       locale="tr-TR"
                       :format-locale="tr"
                       format="dd/MM/yyyy">
        </VueDatePicker>
      </div>
    </div>

    <!-- Protesto Places -->
    <div class="field">
      <label for="ProtestoPlaceIds">Yerler</label>
      <multiselect
          id="ProtestoPlaceIds"
          v-model="this.protesto.protestoPlaceIds"
          :options="protestoPlaceOptions"
          :multiple="true"
          :close-on-select="false"
          :clear-on-select="false"
          :preserve-search="true"
          placeholder="Seçiniz"
          label="name"
          track-by="id"
      ></multiselect>
    </div>

    <!-- Gender -->
    <div class="field">
      <label for="GenderId">Cinsiyet</label>
      <select v-model="this.protesto.genderId" id="GenderId">
        <option value="">--Seçiniz--</option>
        <option
            v-for="gender in genderOptions"
            :key="gender.id"
            :value="gender.id"
        >
          {{ gender.name }}
        </option>
      </select>
    </div>

  <div class="field">
    <label for="CustodyCount">Gözaltı Sayısı</label>
    <input
        type="number"
        id="CustodyCount"
        v-model="this.protesto.custodyCount"
    />
  </div>
    <!-- Other Fields -->
    <div class="two fields">
      <div class="field">
        <label for="EmployeeCountInProtesto">Eylemdeki İşçi Sayısı (Tam)</label>
        <input
            type="number"
            id="EmployeeCountInProtesto"
            v-model="this.protesto.employeeCountInProtesto"
        />
      </div>
      <div class="field">
        <label for="EmployeeCountInProtestoId">Eylemdeki İşçi Sayısı</label>
        <select v-model="this.protesto.employeeCountInProtestoId" id="EmployeeCountInProtestoId">
          <option value="">--Seçiniz--</option>
          <option
              v-for="employeeCountInProtesto in employeeCountInProtestoOptions"
              :key="employeeCountInProtesto.id"
              :value="employeeCountInProtesto.id"
          >
            {{ employeeCountInProtesto.name }}
          </option>
        </select>
      </div>
    </div>

    <!-- Notes -->
    <div class="field">
      <label for="Note">Notlar</label>
      <textarea id="Note" v-model="this.protesto.note" rows="3"></textarea>
    </div>

    <!-- Save Button -->
   
</template>

<script>
import Multiselect from "vue-multiselect";
import VueDatePicker from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css'
import {fetchWithToken} from "../../fetchWrapper";

export default {
  name: "Protesto",
  components: { Multiselect, VueDatePicker },
  props: {
    protesto: {
      type: Object,
      default: () => ({})  // Prevents undefined errors
    },
    protestoTypeOptions: {
      type: Array,
      required: true,
    },
    protestoPlaceOptions: {
      type: Array,
      required: true,
    },
    genderOptions: {
      type: Array,
      required: true,
    },
    employeeCountInProtestoOptions: {
      type: Array,
      required: true,
    }
  }
};
</script>

<style scoped>
/* Add any scoped styles here if necessary */
</style>
