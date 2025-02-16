<template>
    <div  v-for="(location, index) in protestoLocations" :key="index" class="fields" v-show="!location.deleted">
      <!-- City Dropdown -->
      <div class="four field">
          <select v-model="location.cityId">
            <option value="">--İl--</option>
            <option v-for="city in cities" :key="city.id" :value="city.id">{{ city.name }}</option>
          </select>
        </div>
  
        <div class="four field">
        <!-- District Dropdown -->
          <select v-model="location.districtId" >
            <option value="">--İlçe--</option>
            <option v-for="district in this.districts.filter(s=>s.cityId === location.cityId)" :key="district.id" :value="district.id">{{ district.name }}</option>
          </select>
        </div>
        <div class="four field">
          <!-- Place Input -->
          <input v-model="location.place" type="text" />  
        </div>
        <div class="four field">
          <button type="button" @click="deleteLocation(index)" class="ui icon button red basic"><i class="trash icon"></i></button>
        </div>
    </div>
</template>

<script>

export default {
  emits: ['deleteLocation'],
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
  },
  data() {
    return {
      filteredDistricts : []
    }
  },
  methods: {
    deleteLocation(index) {
      this.$emit('deleteLocation', index);
    },
  }
};
</script>

<style scoped>
/* Add styles if necessary */
</style>
