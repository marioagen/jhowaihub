<template>
    <div class="row">
        <div class="col-12 mb-3">
            <div class="input-group">
                <span class="input-group-text border-end-0">
                    <LucideIcon
                        icon="Search"
                        size="16"
                    />
                </span>
                <input
                    id="InputSearch"
                    type="text"
                    class="form-control form-control-sm border-start-0 custom-input"
                    :class="{
                        'border-end-0': showCleanBtn,
                    }"
                    v-model="filters.input"
                    @keydown.enter="setFilters"
                    @keydown.delete="setFilters"
                    :placeholder="$t('filters.templateInput')"
                    ref="searchInpt"
                />
                <span
                    v-if="showCleanBtn"
                    class="input-group-text border-start-0"
                    @click="cleanInput"
                >
                    <LucideIcon
                        icon="X"
                        size="16"
                    />
                </span>
            </div>
        </div>
        <div class="col-12 col-md-3 mb-3 mb-md-0">
            <div class="input-group">
                <span class="input-group-text border-end-0">
                    <LucideIcon
                        icon="ArrowUpDown"
                        size="16"
                    />
                </span>
                <select
                    class="form-select form-select-sm border-start-0"
                    v-model="filters.orderBy"
                    @change="setFilters"
                >
                    <option
                        v-for="sorting in sortingList"
                        :key="sorting.id"
                        :value="sorting.value"
                    >
                        {{ $t(sorting.name) }}
                    </option>
                </select>
            </div>
        </div>
        <div class="col-12 col-md-3">
            <div class="input-group">
                <span class="input-group-text border-end-0">
                    <LucideIcon
                        icon="Zap"
                        size="16"
                    />
                </span>
                <select
                    class="form-select form-select-sm border-start-0"
                    @change="setFilters"
                    v-model="filters.method"
                >
                    <option value="">
                        {{ $t("filters.templates.all") }}
                    </option>
                    <option
                        v-for="method in methodsList"
                        :key="method.id"
                        :value="method.name"
                    >
                        {{ method.name }}
                    </option>
                </select>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "TemplateFilters",
        props: {
            sortingList: {
                type: Array,
                required: false,
                default: () => [
                    {
                        id: 1,
                        name: "filters.mostRecent",
                        value: "created desc",
                    },
                    {
                        id: 2,
                        name: "filters.mostOld",
                        value: "created asc",
                    },
                    {
                        id: 3,
                        name: "filters.nameAZ",
                        value: "name asc",
                    },
                    {
                        id: 4,
                        name: "filters.nameZA",
                        value: "name desc",
                    },
                ],
            },
            methodsList: {
                type: Array,
                required: false,
                default: () => [
                    { id: 1, name: "GET" },
                    { id: 2, name: "POST" },
                    { id: 3, name: "PUT" },
                    { id: 4, name: "PATCH" },
                    { id: 5, name: "DELETE" },
                ],
            },
        },
        data() {
            return {
                filters: {
                    orderBy: "created asc",
                    input: null,
                    method: "",
                },
            };
        },
        methods: {
            setFilters() {
                this.$emit("setFilters", this.filters);
            },
            cleanInput() {
                this.filters.input = null;
                this.setFilters();
            },
        },
        computed: {
            showCleanBtn() {
                return this.filters.input !== null && this.filters.input !== "";
            },
            hasTemplates() {
                return this.templatesList.length > 0;
            },
        },
    };
</script>
<style scooped>
    .custom-input {
        font-size: 12px;
    }

    .custom-input::placeholder {
        font-size: 12px;
        color: #999;
    }
</style>
