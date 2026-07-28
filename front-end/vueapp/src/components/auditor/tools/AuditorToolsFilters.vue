<template>
    <div class="mb-2 d-flex flex-column gap-2">
        <div class="input-group input-group-sm">
            <span class="input-group-text border-end-0 py-1">
                <LucideIcon icon="Search" :size="14" />
            </span>
            <input
                v-model="search"
                type="text"
                class="form-control form-control-sm border-start-0 py-1"
                :placeholder="$t('auditor.tools.filters.searchPlaceholder')"
                :aria-label="$t('auditor.tools.filters.searchAria')"
                @input="emitFilter"
            />
            <button
                v-if="search"
                type="button"
                class="btn btn-outline-secondary btn-sm border-start-0 py-1"
                @click="clearSearch"
            >
                <LucideIcon icon="X" :size="12" />
            </button>
        </div>
        <select
            v-model="category"
            class="form-select form-select-sm"
            @change="emitFilter"
        >
            <option value="">{{ $t("auditor.tools.filters.allCategories") }}</option>
            <option value="agent">{{ $t("auditor.tools.categories.agent") }}</option>
            <option value="connector">{{ $t("auditor.tools.categories.connector") }}</option>
            <option value="apiTemplate">{{ $t("auditor.tools.categories.apiTemplate") }}</option>
            <option value="questionnaire">{{ $t("auditor.tools.categories.questionnaire") }}</option>
        </select>
    </div>
</template>
<script>
    export default {
        name: "AuditorToolsFilters",
        emits: ["filter"],
        data() {
            return { search: "", category: "" };
        },
        methods: {
            emitFilter() {
                this.$emit("filter", { search: this.search, category: this.category });
            },
            clearSearch() {
                this.search = "";
                this.emitFilter();
            },
        },
    };
</script>
