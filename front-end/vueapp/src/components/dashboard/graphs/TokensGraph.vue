<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="mb-0 fw-bold">Consumo Diário de Tokens</h6>
                <div class="d-flex align-items-center gap-2">
                    <LucideIcon icon="ChevronLeft" :size="17" />
                    <span class="mb-0">IA Número 2</span>
                    <LucideIcon icon="ChevronRight" :size="17" />
                </div>
            </div>
            <div class="card ms-4 me-4 mb-3">
                <div class="card-body">
                    <h6>Total de Tokens consumidos</h6>
                    <h4 class="mb-0 fw-bold">4896,11</h4>
                    <span>Valor unitários no plano atual: 0,0001</span>
                    <hr/>
                    <span class="mt-1">Totalizador no período</span>
                    <h4 class="mb-0 fw-bold text-primary">0,15</h4>
                </div>
            </div>
            <h6>Consumo Diário de Tokens</h6>
            <BarGraphComponent
                :options="graph.options"
                :series="graph.series"
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
            graph: {
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
            },
        })
    }
</script>