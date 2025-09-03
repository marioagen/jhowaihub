<template>
    <div class="row">
        <div class="col-10">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="Search" size="16" />
                </span>
                <input
                    id="InputSearch"
                    type="text"
                    class="form-control form-control-sm border-start-0 custom-input"
                    :class="{ 'border-end-0': showCleanBtn }"
                    v-model="filters.Input"
                    @keydown.enter="filterData"
                    @keydown.delete="filterData"
                    :placeholder="$t('filters.workflowInput')"
                    ref="searchInpt"
                />
                <span v-if="showCleanBtn" class="input-group-text border-start-0 bg-white" @click="cleanInput">
                    <LucideIcon icon="X" size="16" />
                </span>
            </div>
        </div>
        <div class="col-1">
            <button
                v-tooltip="filters.IsAllUsers ? $t('filters.assignment.allUsers') : $t('filters.assignment.currentUser')"
                class="btn table-btn btn-sm"
                :class="filters.IsAllUsers ? 'btn-outline-secondary' : 'btn-outline-primary'"
                type="button"
                style="display: flex; align-items: center; justify-content: center;"
                @click="filterUsers"
            >
                <LucideIcon icon="User" />
            </button>
        </div>
    </div>
</template>

<script>
    export default {
        name: "WorkflowFilters",
        data() {
            return {
                teamsList: [],
                filters: {
                    Input: "",
                    IsAllUsers: false,
                }
            };
        },
        methods: {
            filterData() {
                this.$emit("filter", this.filters)
            },
            filterUsers() {
                this.filters.IsAllUsers = !this.filters.IsAllUsers;
                this.filterData();
                console.log(this.filters)
            },
            cleanInput() {
                this.filters.Input = "";
                this.filterData();
            },
        },
        computed: {
            showCleanBtn() {
                return this.filters.Input !== "";
            },
        },
        created() {
        }
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
