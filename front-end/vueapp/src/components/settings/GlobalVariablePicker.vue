<template>
    <div class="global-variable-picker">
        <select
            v-model="selectedName"
            class="form-select form-select-sm"
            :aria-label="$t('settings.globalVariables.picker.label')"
            :disabled="disabled || !variables.length"
        >
            <option value="">
                {{
                    $t(
                        variables.length
                            ? "settings.globalVariables.picker.placeholder"
                            : "settings.globalVariables.picker.empty",
                    )
                }}
            </option>
            <option v-for="variable in variables" :key="variable.id" :value="variable.name">
                {{ variable.name }} · {{ $t(`settings.globalVariables.types.${variable.valueType}`) }}
            </option>
        </select>
        <button
            type="button"
            class="btn btn-outline-primary btn-sm global-variable-picker__insert"
            :disabled="disabled || !selectedName"
            :title="$t('settings.globalVariables.picker.insert')"
            :aria-label="$t('settings.globalVariables.picker.insert')"
            @click="insert"
        >
            <LucideIcon icon="Braces" :size="15" />
            <span>{{ $t("settings.globalVariables.picker.insert") }}</span>
        </button>
    </div>
</template>

<script>
    import { findAvailableGlobalVariables } from "@/services/settings/globalVariablesSettings";

    export default {
        name: "GlobalVariablePicker",
        emits: ["insert"],
        props: {
            context: {
                type: String,
                required: true,
            },
            disabled: {
                type: Boolean,
                default: false,
            },
        },
        data() {
            return {
                selectedName: "",
            };
        },
        computed: {
            variables() {
                return findAvailableGlobalVariables(this.context);
            },
        },
        methods: {
            insert() {
                if (!this.selectedName) return;
                this.$emit("insert", `{{global:${this.selectedName}}}`);
                this.selectedName = "";
            },
        },
    };
</script>

<style scoped>
    .global-variable-picker {
        display: flex;
        align-items: stretch;
        gap: 0.5rem;
        margin-top: 0.4rem;
    }

    .global-variable-picker .form-select {
        min-width: 0;
    }

    .global-variable-picker__insert {
        display: inline-flex;
        min-height: 32px;
        align-items: center;
        justify-content: center;
        gap: 0.35rem;
        white-space: nowrap;
    }

    @media (max-width: 576px) {
        .global-variable-picker {
            flex-direction: column;
        }

        .global-variable-picker__insert {
            min-height: 44px;
        }
    }
</style>