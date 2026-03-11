<template>
    <div class="card mt-1 mb-1">
        <div class="d-flex">
            <div
                class="border-end p-2"
                style="width: 140px"
            >
                <button
                    class="btn btn-sm w-100 text-start mb-1 date-preset-btn"
                    :class="{
                        'date-preset-selected':
                            selectedPreset ===
                            'currentMonth',
                    }"
                    @click="setCurrentMonth"
                >
                    Este mês
                </button>
                <button
                    class="btn btn-sm w-100 text-start mb-1 date-preset-btn"
                    :class="{
                        'date-preset-selected':
                            selectedPreset === 'lastMonth',
                    }"
                    @click="setLastMonth"
                >
                    Mês passado
                </button>
                <button
                    class="btn btn-sm w-100 text-start mb-1 date-preset-btn"
                    :class="{
                        'date-preset-selected':
                            selectedPreset ===
                            'previousSeven',
                    }"
                    @click="setPreviousSeven"
                >
                    Últimos 7 dias
                </button>
                <button
                    class="btn btn-sm w-100 text-start mb-1 date-preset-btn"
                    :class="{
                        'date-preset-selected':
                            selectedPreset ===
                            'previousNinety',
                    }"
                    @click="setPreviousNinety"
                >
                    Últimos 90 dias
                </button>
            </div>
            <div class="p-3 flex-grow-1">
                <div class="mb-3">
                    <label class="form-label">
                        Data de Início
                    </label>
                    <input
                        type="date"
                        class="form-control form-control-sm"
                        v-model="start"
                    />
                </div>
                <div class="mb-3">
                    <label class="form-label">
                        Data Final
                    </label>
                    <input
                        type="date"
                        class="form-control form-control-sm"
                        v-model="end"
                    />
                </div>
                <hr />
                <div
                    class="d-flex justify-content-end gap-2 mt-3"
                >
                    <button
                        class="btn btn-light btn-sm"
                        @click="$emit('close')"
                    >
                        Cancelar
                    </button>
                    <button
                        class="btn btn-primary btn-sm"
                        @click="filter"
                    >
                        Aplicar
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "DashboardDateFilter",
        data() {
            const today = new Date();
            const first = new Date(
                today.getFullYear(),
                today.getMonth(),
                1
            );
            return {
                selectedPreset: "currentMonth",
                start: first.toISOString().slice(0, 10),
                end: today.toISOString().slice(0, 10),
            };
        },
        methods: {
            filter() {
                this.filterData();
                this.$emit("close");
            },
            filterData() {
                this.$emit("filterData", {
                    preset: this.selectedPreset,
                    start: this.start,
                    end: this.end,
                });
            },
            setCurrentMonth() {
                this.selectedPreset = "currentMonth";
                const today = new Date();
                const first = new Date(
                    today.getFullYear(),
                    today.getMonth(),
                    1
                );
                this.start = first
                    .toISOString()
                    .slice(0, 10);
                this.end = today.toISOString().slice(0, 10);
                this.filterData();
            },
            setLastMonth() {
                this.selectedPreset = "lastMonth";
                const now = new Date();
                const first = new Date(
                    now.getFullYear(),
                    now.getMonth() - 1,
                    1
                );
                const last = new Date(
                    now.getFullYear(),
                    now.getMonth(),
                    0
                );
                this.start = first
                    .toISOString()
                    .slice(0, 10);
                this.end = last.toISOString().slice(0, 10);
                this.filterData();
            },
            setPreviousSeven() {
                this.selectedPreset = "previousSeven";
                const end = new Date();
                const start = new Date();
                start.setDate(end.getDate() - 7);
                this.start = start
                    .toISOString()
                    .slice(0, 10);
                this.end = end.toISOString().slice(0, 10);
                this.filterData();
            },
            setPreviousNinety() {
                this.selectedPreset = "previousNinety";
                const end = new Date();
                const start = new Date();
                start.setDate(end.getDate() - 90);
                this.start = start
                    .toISOString()
                    .slice(0, 10);
                this.end = end.toISOString().slice(0, 10);
                this.filterData();
            },
        },
    };
</script>
<style>
    .date-preset-btn:focus {
        outline: none !important;
        box-shadow: none !important;
    }

    .date-preset-btn {
        color: var(--color-body-content) !important;
    }

    .date-preset-selected {
        background-color: var(--color-bg-sidebar-li-selected) !important;
        color: var(--color-body-content) !important;
        font-weight: 500;
    }
</style>
