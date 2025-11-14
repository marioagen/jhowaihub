<template>
    <div class="card mb-3">
        <div class="card-body">
            <h5>Consumo Diário de Tokens</h5>
            <BarGraphComponent
                :options="options"
                :series="series"
            />
        </div>
    </div>
</template>

<script>
    import BarGraphComponent from '@/components/global/graphs/BarGraphComponent.vue';
    export default {
        components: {
            BarGraphComponent,
        },
        data: () => ({
            options: {
                chart: {
                    id: 'sales-bar',
                    toolbar: { 
                        show: false
                    },
                },
                plotOptions: {
                    bar: {
                        borderRadius: 5,
                    },
                },
                dataLabels: {
                    enabled: false,
                },
                xaxis: {
                    categories: [
                        '01/11', '02/11', '03/11', '04/11', '05/11',
                        '06/11', '07/11', '08/11', '09/11', '10/11',
                        '11/11', '12/11', '13/11', '14/11', '15/11'
                    ]
                },
                colors: ['#0060c7']
            },
            series: [
                {
                    name: 'Online Sales',
                    data: [90, 30, 65, 50, 70, 45, 80, 55, 60, 75, 40, 85, 50, 95, 60]
                }
            ]
        })
    }
</script>