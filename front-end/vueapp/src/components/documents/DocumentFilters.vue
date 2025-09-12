<template>
    <div class="row">
       <div class="col">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="Search" size="16" />
                </span>
                <input
                    id="InputSearch"
                    type="text"
                    class="form-control form-control-sm border-start-0 custom-input"
                    :class="{ 'border-end-0': showCleanBtn }"
                    v-model="filters.input"
                    @keydown.enter="filterData"
                    @keydown.delete="filterData"
                    :placeholder="$t('filters.documentInput')"
                    ref="searchInpt"
                />
                <span v-if="showCleanBtn" class="input-group-text border-start-0 bg-white" @click="cleanInput">
                    <LucideIcon icon="X" size="16" />
                </span>
            </div>
        </div>
        <div class="col-auto">
            <select
                v-model="filters.teamId"
                class="form-select form-select-sm w-auto"
                @change="filterData"
            >
                <option value="" disabled>{{ $t("filters.teamsSelect.none") }}</option>
                <option :value="0">{{ $t("filters.teamsSelect.all") }}</option>
                <option 
                    v-for="team in teamsList" 
                    :key="team.id" 
                    :value="team.id"
                >
                    {{ team.name }}
                </option>
            </select>
        </div>
        <!-- <div class="col-1">
            <button
                v-tooltip="filters.isAllUsers ? $t('filters.assignment.allUsers') : $t('filters.assignment.currentUser')"
                class="btn table-btn btn-sm"
                :class="filters.isAllUsers ? 'btn-outline-secondary' : 'btn-outline-primary'"
                type="button"
                style="display: flex; align-items: center; justify-content: center;"
                @click="filterUsers"
            >
                <LucideIcon icon="User" />
            </button>
        </div> -->
    </div>
</template>

<script>
    export default {
        name: "DocumentFilters",
        props: {
            teamsList: { type: Array, required: true } // prop vinda do pai
        },
        data() {
            return {
                filters: {
                    input: "",
                    teamId: "",
                    teams: [],
                    isAllUsers: false,
                }
            };
        },
        watch: {
            teamsList: {
                immediate: true, 
                handler(newVal) {
                    if (newVal.length) {
                        this.filters.teams = this.filters.teamId
                            ? [this.filters.teamId]
                            : newVal.map(t => t.id);
                            
                        this.$emit("filter", { ...this.filters });
                    }
                }
            }
        },
        methods: {
            filterData() {
                this.filters.teams = this.filters.teamId
                    ? [this.filters.teamId]                
                    : this.teamsList.map(t => t.id);      
                this.$emit("filter", { ...this.filters });
            },
            filterUsers() {
                this.filters.isAllUsers = !this.filters.isAllUsers;
                this.filterData();
            },
            cleanInput() {
                this.filters.input = "";
                this.filterData();
            },
        },
        computed: {
            showCleanBtn() {
                return this.filters.input !== "";
            },
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
