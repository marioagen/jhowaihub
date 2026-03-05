<template>
    <div class="row">
        <div class="col-6">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
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
                    @keydown.enter="filterData"
                    @keydown.delete="filterData"
                    :placeholder="$t('filters.workflowInput')"
                    ref="searchInpt"
                />
                <span
                    v-if="showCleanBtn"
                    class="input-group-text border-start-0 bg-white"
                    @click="cleanInput"
                >
                    <LucideIcon
                        icon="X"
                        size="16"
                    />
                </span>
            </div>
        </div>
        <div class="col-3">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon
                        icon="ArrowUpDown"
                        size="16"
                    />
                </span>
                <select
                    class="form-select form-select-sm border-start-0"
                    v-model="filters.orderBy"
                    @change="filterData"
                >
                    <option value="created desc">
                        {{ $t("filters.mostRecent") }}
                    </option>
                    <option value="created asc">
                        {{ $t("filters.mostOld") }}
                    </option>
                    <option value="name asc">
                        {{ $t("filters.nameAZ") }}
                    </option>
                    <option value="name desc">
                        {{ $t("filters.nameZA") }}
                    </option>
                </select>
            </div>
        </div>
        <div class="col-3">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon
                        icon="FileText"
                        size="16"
                    />
                </span>
                <select
                    class="form-select form-select-sm border-start-0"
                    v-model="filters.document"
                    @change="filterData"
                >
                    <option value="1">
                        {{ $t("filters.all") }}
                    </option>
                    <option value="2">
                        {{ $t("filters.singleDocuments") }}
                    </option>
                    <option value="3">
                        {{ $t("filters.batchDocuments") }}
                    </option>
                </select>
            </div>
        </div>
    </div>
</template>
<script>
    export default {
        name: "WorkflowViewFilters",
        props: {
            teamsList: {
                type: Array,
                required: false,
                default: () => [],
            },
            usersList: {
                type: Array,
                required: false,
                default: () => [],
            },
        },
        data() {
            return {
                filters: {
                    orderBy: "created desc",
                    input: null,
                    isAllUsers: true,
                    login: this.$store.state.userProfile.login,
                    document: "1",
                },
            };
        },
        methods: {
            filterData() {
                this.$emit("filter", this.filters);
            },
            filterUsers() {
                this.filters.isAllUsers = !this.filters.isAllUsers;
                this.filterData();
            },
            cleanInput() {
                this.filters.input = null;
                this.filterData();
            },
        },
        computed: {
            showCleanBtn() {
                return this.filters.input !== null;
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
