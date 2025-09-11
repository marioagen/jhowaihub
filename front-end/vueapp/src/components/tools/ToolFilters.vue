<template>
    <div class="row">
        <div class="col-10">
            <div class="input-group">
                <span class="input-group-text border-end-0 bg-white">
                    <LucideIcon icon="Search" :size="16" />
                </span>
                <input
                    id="InputSearch"
                    type="text"
                    class="form-control form-control-sm border-start-0 custom-input"
                    :class="{ 'border-end-0': showCleanBtn }"
                    v-model="filters.input"
                    @keydown.enter="filterData"
                    @keydown.delete="filterData"
                    :placeholder="$t('filters.toolInput')"
                    ref="searchInpt"
                />
                <span v-if="showCleanBtn" class="input-group-text border-start-0 bg-white" @click="cleanInput">
                    <LucideIcon icon="X" size="16" />
                </span>
            </div>
        </div>
        <div class="col-2">
            <select
                class="form-select form-select-sm w-auto"
                v-model="filters.toolTypeId"
                @change="filterData"
            >
                <option value="" disabled>{{ $t("filters.typesSelect.none") }}</option>
                <option :value="0">{{ $t("filters.typesSelect.all") }}</option>
                <option 
                    v-for="team in toolsTypesList"
                    :key="team.id" 
                    :value="team.id"
                >
                    {{ team.name }}
                </option>
            </select>
        </div>
    </div>
</template>

<script>
    import TeamsService from '@/services/teams/TeamsService';
    export default {
        name: "DocumentFilters",
        data() {
            return {
                teamsList: [],
                filters: {
                    input: "",
                    toolTypeId: "",
                }
            };
        },
        methods: {
            getTeams() {
                TeamsService.getTeamsByUser()
                    .then((response) => {
                        this.teamsList = response;
                    });
            },
            filterData() {
                this.$emit("filter", this.filters)
            },
            cleanInput() {
                this.filters = {
                    input: "",
                    toolTypeId: "",
                };
                this.filterData();
            },
        },
        computed: {
            showCleanBtn() {
                return this.filters.input !== "";
            },
        },
        created() {
            this.getTeams();
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
