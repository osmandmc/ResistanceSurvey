<template>
    <!-- Protesto Types -->
    <div class="field">
      <label for="ProtestoTypeIds">Eylem Türleri</label>
      <multiselect
          id="ProtestoTypeIds"
          v-model="protesto.protestoTypeIds"
          :options="protestoTypeOptions"
          :multiple="true"
          :close-on-select="false"
          :clear-on-select="false"
          :preserve-search="true"
          placeholder="Seçiniz"
          label="name"
          track-by="id"
          @select="clearError('protestoTypeIds')"
      ></multiselect>
      <span v-if="formErrors.protestoTypeIds" class="field error">
        <label>{{ formErrors.protestoTypeIds }}</label>
      </span>
    </div>
  <div class="field" v-if="showStrikeDuration">
    <label for="StrikeDuration">Eylemin Süresi</label>
    <input v-model="protesto.strikeDuration" type="text" />
  </div>
  <div class="field" v-show="showSimpleProtestoDescription">
    <label for="SimpleProtestoDescription">İş yerinde Basit Eylem Açıklama</label>
    <input v-model="protesto.simpleProtestoDescription" type="text" />
  </div>
    <!-- Start and End Dates -->
    <div class="two fields">
      <div class="field">
        <label for="ProtestoStartDate">Başlangıç Tarihi</label>
        <div class="custom-datepicker-wrapper">
          <VueDatePicker v-model="protesto.protestoStartDate" 
                         text-input 
                         locale="tr-TR"
                         format="dd.MM.yyyy"
                         @blur="clearError('protestoStartDate')"
          >
          </VueDatePicker>
        </div>
        <span v-if="formErrors.protestoStartDate" class="field error">
          <label>{{ formErrors.protestoStartDate }}</label>
        </span>
      </div>
      <div class="field">
        <label for="ProtestoEndDate">Bitiş Tarihi</label>
        <div class="custom-datepicker-wrapper">
          <VueDatePicker v-model="protesto.protestoEndDate" 
                         text-input
                         locale="tr-TR"
                         format="dd.MM.yyyy">
          </VueDatePicker>
        </div>
      </div>
    </div>

  
    <!-- Protesto Places -->
    <div class="field">
      <label for="ProtestoPlaceIds">Eylem Yerleri</label>
      <multiselect
          id="ProtestoPlaceIds"
          v-model="protesto.protestoPlaceIds"
          :options="protestoPlaceOptions"
          :multiple="true"
          :close-on-select="false"
          :clear-on-select="false"
          :preserve-search="true"
          placeholder="Seçiniz"
          label="name"
          track-by="id"
          @select="clearError('protestoPlaceIds')"
      ></multiselect>
      <span v-if="formErrors.protestoPlaceIds" class="field error">
        <label>{{ formErrors.protestoPlaceIds }}</label>
      </span>
    </div>

    <!-- Gender -->
    <div class="field">
      <label for="GenderId">Cinsiyet</label>
      <select v-model="protesto.genderId" @change="clearError('genderId')">
        <option value="">--Seçiniz--</option>
        <option
            v-for="gender in genderOptions"
            :key="gender.id"
            :value="gender.id"
        >
          {{ gender.name }}
        </option>
      </select>
      <span v-if="formErrors.genderId" class="field error">
        <label>{{ formErrors.genderId }}</label>
      </span>
    </div>
  <div class="field">
    <button type="button" class="ui button" @click="addLocation">Lokasyon Ekle</button>
  </div>
  <div class="field">
    <Location @deleteLocation="handleDeleteLocation" :cities="cities" :districts="districts" :protesto-locations="protesto.locations" />
  </div>
 
    <!-- Other Fields -->
    <div class="two fields">
      <div class="field">
        <label for="EmployeeCountInProtesto">Eylemdeki İşçi Sayısı (Tam)</label>
        <input
            type="number"
            id="EmployeeCountInProtesto"
            v-model="protesto.employeeCountInProtesto"
            @blur="updateEmployeeCountId"
        />
      </div>
      <div class="field">
        <label for="EmployeeCountInProtestoId">Eylemdeki İşçi Sayısı</label>
        <select v-model="protesto.employeeCountInProtestoId" id="EmployeeCountInProtestoId">
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
    <div class="field">
      <label for="InterventionTypeId">Müdahale Tipi</label>
      <multiselect
          id="ProtestoPlaceIds"
          v-model="protesto.interventionTypeIds"
          :options="interventionTypes"
          :multiple="true"
          :close-on-select="false"
          :clear-on-select="false"
          :preserve-search="true"
          placeholder="Seçiniz"
          label="name"
          track-by="id"
          @select="clearError('interventionTypeIds')"
      ></multiselect>
      <span v-if="formErrors.interventionTypeIds" class="field error">
          <label>{{ formErrors.interventionTypeIds }}</label>
      </span>
    </div>
    <div class="field" v-show="isCustodyPossible">
      <label for="CustodyCount">Gözaltı Sayısı</label>
      <input type="text" id="CustodyCount" v-model="this.protesto.custodyCount" 
             @input="clearError('custodyCount')" />
      <span v-if="formErrors.custodyCount" class="field error">
          <label>{{ formErrors.custodyCount }}</label>
      </span>
    </div>
  
    <!-- Notes -->
    <div class="field">
      <label for="Note">Kontrol Kişisine Notlar</label>
      <textarea id="Note" v-model="protesto.note" rows="3"></textarea>
    </div>

    <!-- Save Button -->
   
</template>

<script>
import Multiselect from "vue-multiselect";
import VueDatePicker from '@vuepic/vue-datepicker';
import '@vuepic/vue-datepicker/dist/main.css'
import Location from "./Location.vue";

export default {
  name: "Protesto",
  emits: ["addProtesto", 'deleteLocation', 'addLocation', "onInputChanged"],
  components: { Multiselect, VueDatePicker, Location },
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
    },
    cities: {
      type: Array,
      required: true,
    },
    districts: {
      type: Array,
      required: true,
    },
    interventionTypes: {
      type: Array,
      required: true,
    },
    formErrors: {
      type:Object,
      default: () => ({})
    }
  },
  computed: {
    isCustodyPossible() {
      return this.protesto.interventionTypeIds &&
        !this.protesto.interventionTypeIds.some(type => type.id === 7)
    },
    showSimpleProtestoDescription() {
      return (
          this.protesto.protestoTypeIds &&
          this.protesto.protestoTypeIds.some(type => type.id === 35)
      );
    },
    showStrikeDuration() {
      return (
          this.protesto.protestoTypeIds &&
          this.protesto.protestoTypeIds.some(type => [5, 6].includes(type.id))
      );
    }
  },
  methods: {
    addLocation() {
      this.$emit('addLocation');
    },
    handleDeleteLocation(index) {
      console.log(index);
      this.$emit('deleteLocation', index);
    },
    updateEmployeeCountId() {
      const employeecount = this.protesto.employeeCountInProtesto;
      if (employeecount == null) return;

      if (employeecount >= 1 && employeecount <= 5) {
        this.protesto.employeeCountInProtestoId = 1;
      } else if (employeecount >= 6 && employeecount <= 25) {
        this.protesto.employeeCountInProtestoId = 2;
      } else if (employeecount >= 26 && employeecount <= 50) {
        this.protesto.employeeCountInProtestoId = 3;
      } else if (employeecount >= 51 && employeecount <= 100) {
        this.protesto.employeeCountInProtestoId = 4;
      } else if (employeecount >= 101 && employeecount <= 250) {
        this.protesto.employeeCountInProtestoId = 5;
      } else if (employeecount >= 251 && employeecount <= 500) {
        this.protesto.employeeCountInProtestoId = 6;
      } else if (employeecount >= 501 && employeecount <= 1000) {
        this.protesto.employeeCountInProtestoId = 7;
      } else if (employeecount >= 1001 && employeecount <= 2500) {
        this.protesto.employeeCountInProtestoId = 8;
      } else if (employeecount >= 2501 && employeecount <= 5000) {
        this.protesto.employeeCountInProtestoId = 9;
      } else if (employeecount >= 5001 && employeecount <= 10000) {
        this.protesto.employeeCountInProtestoId = 10;
      } else if (employeecount >= 10001 && employeecount <= 25000) {
        this.protesto.employeeCountInProtestoId = 11;
      } else if (employeecount >= 25001 && employeecount <= 50000) {
        this.protesto.employeeCountInProtestoId = 12;
      } else if (employeecount >= 50001 && employeecount <= 100000) {
        this.protesto.employeeCountInProtestoId = 13;
      } else if (employeecount >= 100001) {
        this.protesto.employeeCountInProtestoId = 14;
      } else {
        this.protesto.employeeCountInProtestoId = null; // Reset if out of range
      }
    },
    clearError(field) {
      // Clear the error message for the specified field
      console.log(field);
      this.$emit('onInputChanged', field);
    },
  }
};
</script>

<style scoped>
.custom-datepicker-wrapper >>> .dp__input_icon_pad {
  padding-inline-start: var(--dp-input-icon-padding) !important;
}
</style>
