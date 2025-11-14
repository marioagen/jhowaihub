<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-2 mb-3">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <div>
                        <h5 class="mb-0 fw-bold">{{ $t("dashboard.title") }}</h5>
                        <p>
                            <small class="text-muted">{{ $t("dashboard.subtitle") }}</small>
                        </p>
                    </div>
                    <div class="plan-box plan-right">
                        <small class="text-muted">Plano Atual</small><br>
                        <span class="plan-title">Plano Enterprise</span>
                    </div>
                </div>
            </div>
            <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="row">
                    <div class="col">
                        <button
                            class="btn btn-outlined-light btn-sm border d-flex align-items-center justify-content-between"
                            style="width: 200px;"
                            @click="toggleDateFilter"
                        >
                            {{ label }}
                            Esse mês
                            <LucideIcon v-if="showDateFilter" icon="ChevronUp" :size="17" />
                            <LucideIcon v-else icon="ChevronDown" :size="17" />
                        </button>
                    </div>
                    <div class="col">
                        <button class="btn btn-primary btn-sm">
                            <LucideIcon icon="RefreshCcw" :size="17" />
                            Atualizar
                        </button>
                    </div>
                </div>
                <button class="btn btn-outlined-primary btn-sm">
                    <LucideIcon icon="ArrowDownToLine" :size="17" />
                    Exportar CSV
                </button>
            </div>
            <div class="card mb-3">
                <div class="card-body text-center">
                    <div class="d-inline-flex align-items-center justify-content-center gap-1 mb-2">
                        <span class="me-1">Total WTC</span>
                        <LucideIcon icon="Info" :size="17" />
                    </div>
                    <h1 class="total-wtc-value">444,31</h1>
                </div>
            </div>
            <TokensGraph />
        </div>
    </main>
</template>

<script>
    import TokensGraph from '@/components/dashboard/graphs/TokensGraph.vue';
    import DashboardDateFilter from '@/components/dashboard/DashboardDateFilter.vue';
    export default {
        components: {
            DashboardDateFilter,
            TokensGraph,
        },
        data: () => ({
            showDateFilter: false,
        }),
        methods: {
            toggleDateFilter() {
                this.showDateFilter = !this.showDateFilter;
            },
        }
    }
</script>

<style scoped>
    .plan-box {
        background: #eef3ff;
        border-radius: 12px;
        padding: 12px 20px;
        display: inline-block;
        text-align: right;
        border: 1px solid #d5e0ff;
    }

    .plan-title {
        color: #0056d2;
        font-weight: 600;
        font-size: 1rem;
    }

    .plan-right {
        margin-left: auto;
        display: block; /* auto funciona apenas com block ou flex item */
        width: fit-content;
    }

    .total-wtc-value {
        color: #0056d2;
        font-weight: 700;
        margin: 0;
    }
</style>