<template>
  <div>
    <div v-for="(location, index) in protestoLocations" :key="index" class="fields">
      <!-- City Dropdown -->
      <select v-model="location.cityId">
        <option value="">--İl--</option>
        <option v-for="city in cities" :key="city.id" :value="city.id">{{ city.name }}</option>
      </select>

      <!-- District Dropdown -->
      <select v-model="location.districtId" >
        <option value="">--İlçe--</option>
        <option v-for="district in districts.filter(s=>s.cityId === location.cityId)" :key="district.id" :value="district.id">{{ district.name }}</option>
      </select>

      <!-- Place Input -->
      <input v-model="location.place" type="text" placeholder="Enter place" />
    </div>
  </div>
</template>

<script>

export default {
  props: {
    protestoLocations: {
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
    }
  }
};
</script>

<style scoped>
/* Add styles if necessary */
</style>
