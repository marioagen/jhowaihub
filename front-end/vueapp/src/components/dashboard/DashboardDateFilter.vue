<template>
    <div class="d-flex">
        <!-- Sidebar -->
        <div class="border-end p-2" style="width: 140px;">
            <button class="btn w-100 text-start mb-1" @click="presetThisMonth">
                Este mês
            </button>
            <button class="btn w-100 text-start mb-1" @click="presetLastMonth">
                Mês passado
            </button>
            <button class="btn w-100 text-start mb-1" @click="preset7">
                Últimos 7 dias
            </button>
            <button class="btn w-100 text-start mb-1" @click="preset90">
                Últimos 90 dias
            </button>
        </div>

        <!-- Content -->
        <div class="p-3 flex-grow-1">
            <div class="mb-3">
                <label class="form-label">Data de Início</label>
                <input type="date" class="form-control" v-model="local.start" />
            </div>
            <div class="mb-3">
                <label class="form-label">Data Final</label>
                <input type="date" class="form-control" v-model="local.end" />
            </div>
            <div class="d-flex justify-content-end gap-2 mt-3">
                <button class="btn btn-light" @click="$emit('cancel')">Cancelar</button>
                <button class="btn btn-primary" @click="filterData">Aplicar</button>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "DashboardDateFilter",
        data() {
            return {
                range: "",
                start: "",
                end: "",
            };
        },
        methods: {
            filterData() {
                this.$emit("filterData", { range: this.range, start: this.start, end: this.end });
            },
            presetThisMonth() {
                const today = new Date();
                const first = new Date(today.getFullYear(), today.getMonth(), 1);
                this.$emit(
                    "preset",
                    {
                        start: first.toISOString().slice(0, 10),
                        end: today.toISOString().slice(0, 10),
                    },
                    "Este mês"
                );
            },
            presetLastMonth() {
                const now = new Date();
                const first = new Date(now.getFullYear(), now.getMonth() - 1, 1);
                const last = new Date(now.getFullYear(), now.getMonth(), 0);
                this.$emit(
                    "preset",
                    {
                        start: first.toISOString().slice(0, 10),
                        end: last.toISOString().slice(0, 10),
                    },
                    "Mês passado"
                );
            },
            preset7() {
                const end = new Date();
                const start = new Date();
                start.setDate(end.getDate() - 7);

                this.$emit(
                    "preset",
                    {
                        start: start.toISOString().slice(0, 10),
                        end: end.toISOString().slice(0, 10),
                    },
                    "Últimos 7 dias"
                );
            },
            preset90() {
                const end = new Date();
                const start = new Date();
                start.setDate(end.getDate() - 90);

                this.$emit(
                    "preset",
                    {
                        start: start.toISOString().slice(0, 10),
                        end: end.toISOString().slice(0, 10),
                    },
                    "Últimos 90 dias"
                );
            },
        },
    };
</script>