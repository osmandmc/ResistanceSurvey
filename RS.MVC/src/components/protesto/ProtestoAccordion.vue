<template>
  <div class="ui styled accordion">
    <div v-for="(protesto, index) in protestoItems" :key="protesto.protestoId">
      <div class="title" @click="toggleAccordion(index)">
        <i class="dropdown icon"></i>
        <span v-for="(protestoType, pIndex) in protesto.protestoTypeIds" :key="pIndex">
          {{ protestoType.name }}
        </span>
        <span> {{ formatDate(protesto.protestoStartDate) }}</span>
      </div>
      <div :class="{ active: activeIndex === index }" class="content">
        <edit-protesto
            :protesto="protesto"
            :genderOptions="genderOptions"
            :protestoTypeOptions="protestoTypeOptions"
            :protestoPlaceOptions="protestoPlaceOptions"
            :employeeCountInProtestoOptions="employeeCountInProtestoOptions"
        />
      </div>
    </div>
  </div>
</template>

<script>

import EditProtesto from "./EditProtesto.vue";

export default {
  name: "ProtestoAccordion",
  components: { EditProtesto },
  props: {
    protestoItems: {
      type: Array,
      required: true,
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
  },
  mounted() {
  },
  data() {
    return {
      activeIndex: null,
    };
  },
  methods: {
    toggleAccordion(index) {
      console.log(index);
      console.log(this.activeIndex);
      this.activeIndex = this.activeIndex === index ? null : index;
      console.log(this.activeIndex);
    },
    saveProtesto(updatedProtesto) {
      this.$emit("updateProtesto", updatedProtesto);
    },
    formatDate(date) {
      return new Date(date).toLocaleDateString(); // Format date to a readable string
    },
  },
};
</script>
