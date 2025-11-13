<template>
    <div>
        <ApexChart
            type="bar"
            height="350"
            :options="options"
            :series="series"
        />
    </div>
</template>

<script>
import ApexChart from 'vue3-apexcharts';
export default {
    name: 'BarGraphComponent',
    components: { 
        ApexChart
    },
    props: {
        series: {
            type: Object,
            required: true,
        },
        options: {
            type: Object,
            required: true,
        },
    },
}
</script>