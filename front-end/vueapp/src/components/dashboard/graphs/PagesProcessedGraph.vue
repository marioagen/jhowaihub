<template>
    <div class="card mb-3">
        <div class="card-body">
            <div class="d-flex align-items-center gap-2 mb-3">
                <h6 class="mb-0 fw-bold">{{ $t("dashboard.graphs.pagesGraphTitle") }}</h6>
                <LucideIcon 
                    v-tooltip.right="$t('dashboard.graphs.pagesTooltip')" 
                    icon="Info" 
                    :size="17" 
                />
            </div>
            <div class="card ms-4 me-4 mb-3">
                <div class="card-body">
                    <h6>{{ $t("dashboard.graphs.totalPages") }}</h6>
                    <h4 class="mb-0 fw-bold">4896,11</h4>
                    <span> {{ $t("dashboard.graphs.unitValue") }} 0,0081</span>
                    <hr/>
                    <span class="mt-1">{{ $t("dashboard.graphs.periodTotal") }}</span>
                    <h4 class="mb-0 fw-bold text-primary">0,15</h4>
                </div>
            </div>
            <h6>{{ $t("dashboard.graphs.pagesGraphSubtitle") }}</h6>
            <LoadingComponent
                v-if="isLoading"
            />
            <BarGraphComponent
                v-else
                :options="graph.options"
                :series="graph.series"
            />
        </div>
    </div>
</template>

<script>
    import BarGraphComponent from '@/components/global/graphs/BarGraphComponent.vue';
    import LoadingComponent from '@/components/global/LoadingComponent.vue';
    import DashboardServices from '@/services/dashboard/DashboardServices';
    export default {
        components: {
            BarGraphComponent,
            LoadingComponent
        },
        props: {
            rangeDates: {
                type: Object,
                required: true,
            },
        },
        data: () => ({
            isLoading: true,
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
                    colors: ['#10315B']
                },
                series: [
                    {
                        name: 'Online Sales',
                        data: [74, 39, 91, 62, 83, 46, 89, 57, 71, 64, 37, 94, 52, 86, 58]
                    }
                ]
            },
        }),
        created() {
            this.getPagesData();
            console.log(this.rangeDates)
        },
        methods: {
            getPagesData() {
                this.isLoading = true;
                DashboardServices.getPagesData()
                    .then((response) => {
                        console.log(response)
                    })
                    .finally(() => {
                        this.isLoading = false;
                    });
            },
        },
    }
</script>