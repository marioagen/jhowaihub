<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex justify-content-between align-items-center mb-3">
                <h6 class="mb-0 fw-bold">Páginas de Documento Processadas (OCR)</h6>
            </div>
            <div class="card ms-4 me-4 mb-3">
                <div class="card-body">
                    <h6>Total de Páginas Processadas</h6>
                    <h4 class="mb-0 fw-bold">4896,11</h4>
                    <span>Valor unitário no plano atual: 0,08</span>
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
                        data: [74, 39, 91, 62, 83, 46, 89, 57, 71, 64, 37, 94, 52, 86, 58]
                    }
                ]
            },
        })
    }
</script>