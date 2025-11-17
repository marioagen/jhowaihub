<template>
    <div class="card mb-3">
        <div class="card-body">
            <div>
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h6 class="mb-0 fw-bold">Execuções de Workflows de Automação de IA</h6>
                </div>
                <div class="card ms-4 me-4 mb-3">
                    <div class="card-body">
                        <h6>Total de Execuções de Automação de IA</h6>
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
            <hr/>
            <div>
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h6 class="mb-0 fw-bold">Execuções de Workflows Woopi AI</h6>
                </div>
                <div class="card ms-4 me-4 mb-3">
                    <div class="card-body">
                        <h6>Total de Execuções Woopi AI</h6>
                        <h4 class="mb-0 fw-bold">4896,11</h4>
                        <span>Valor unitário no plano atual: 0,08</span>
                        <hr/>
                        <span class="mt-1">Totalizador no período</span>
                        <h4 class="mb-0 fw-bold text-primary">0,15</h4>
                    </div>
                </div>
                <h6>Consumo Diário de Tokens</h6>
                <BarGraphComponent
                    :options="graph2.options"
                    :series="graph2.series"
                />
            </div>
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
                        data: [68, 41, 88, 53, 79, 47, 92, 59, 73, 66, 38, 97, 55, 82, 61]
                    }
                ]
            },
            graph2: {
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
                        data: [72, 48, 93, 58, 81, 36, 87, 54, 69, 77, 42, 91, 63, 84, 57]
                    }
                ]
            },
        })
    }
</script>