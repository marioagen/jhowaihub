<template>
    <ModalComponent
        ref="modal"
        id="global-variable-form-modal"
        :title="modalTitle"
        save-text="common.save"
        @save="submit"
        @cancel="resetForm"
    >
        <div class="mb-3">
            <label class="form-label" for="global-variable-name">
                {{ $t("settings.globalVariables.form.name") }}
            </label>
            <input
                id="global-variable-name"
                v-model="form.name"
                type="text"
                class="form-control"
                :class="{ 'is-invalid': nameHasError }"
                autocomplete="off"
                @input="validationError = ''"
            />
            <div v-if="nameHasError" class="invalid-feedback">{{ $t(validationError) }}</div>
            <div v-else class="form-text">{{ $t("settings.globalVariables.form.nameHint") }}</div>
        </div>

        <fieldset class="mb-3">
            <legend class="form-label mb-2">{{ $t("settings.globalVariables.form.valueType") }}</legend>
            <div class="global-variable-form__type-options">
                <label class="global-variable-form__type-option">
                    <input v-model="form.valueType" type="radio" value="common" @change="changeValueType" />
                    <span>
                        <LucideIcon icon="Braces" :size="16" />
                        <strong>{{ $t("settings.globalVariables.types.common") }}</strong>
                        <small>{{ $t("settings.globalVariables.form.commonHint") }}</small>
                    </span>
                </label>
                <label class="global-variable-form__type-option">
                    <input v-model="form.valueType" type="radio" value="environment" @change="changeValueType" />
                    <span>
                        <LucideIcon icon="Container" :size="16" />
                        <strong>{{ $t("settings.globalVariables.types.environment") }}</strong>
                        <small>{{ $t("settings.globalVariables.form.environmentTypeHint") }}</small>
                    </span>
                </label>
            </div>
        </fieldset>

        <div class="mb-3">
            <label class="form-label" for="global-variable-value">
                {{ $t("settings.globalVariables.form.value") }}
            </label>
            <Multiselect
                v-if="isEnvironmentType"
                id="global-variable-value"
                v-model="selectedCommonVariable"
                value-prop="value"
                label="label"
                :options="commonVariableOptions"
                :searchable="true"
                :can-clear="false"
                :can-deselect="false"
                :placeholder="$t('settings.globalVariables.form.environmentPlaceholder')"
                :no-options-text="$t('settings.globalVariables.form.noCommonVariables')"
                :no-results-text="$t('settings.globalVariables.form.noCommonVariableResults')"
            />
            <div v-else-if="isEditing && !isReplacingCommonValue" class="input-group">
                <input
                    id="global-variable-value"
                    type="password"
                    class="form-control"
                    value="existing-value"
                    readonly
                    tabindex="-1"
                    autocomplete="off"
                />
                <button
                    type="button"
                    class="btn btn-outline-danger global-variable-form__clear-value"
                    :title="$t('settings.globalVariables.form.clearValue')"
                    :aria-label="$t('settings.globalVariables.form.clearValue')"
                    @click="clearCommonValue"
                >
                    <LucideIcon icon="Trash2" :size="16" />
                </button>
            </div>
            <div v-else class="input-group">
                <input
                    id="global-variable-value"
                    v-model="form.value"
                    :type="showValue ? 'text' : 'password'"
                    class="form-control"
                    :class="{ 'is-invalid': valueHasError }"
                    autocomplete="new-password"
                    @input="validationError = ''"
                />
                <button
                    v-if="!isEditing"
                    type="button"
                    class="btn btn-outline-secondary global-variable-form__visibility"
                    :title="$t(showValue ? 'settings.globalVariables.form.hide' : 'settings.globalVariables.form.show')"
                    @click="showValue = !showValue"
                >
                    <LucideIcon :icon="showValue ? 'EyeOff' : 'Eye'" :size="16" />
                </button>
            </div>
            <div v-if="valueHasError" class="invalid-feedback d-block">{{ $t(validationError) }}</div>
        </div>

        <div class="mb-3">
            <label class="form-label" for="global-variable-description">
                {{ $t("settings.globalVariables.form.description") }}
            </label>
            <input
                id="global-variable-description"
                v-model="form.description"
                type="text"
                class="form-control"
            />
        </div>

        <p class="global-variable-form__usage mb-0">
            {{ $t("settings.globalVariables.form.usage") }}
            <code>{{ placeholder }}</code>
        </p>
    </ModalComponent>
</template>

<script>
    import Multiselect from "@vueform/multiselect";
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import {
        findCommonGlobalVariables,
        globalVariableNameExists,
        isValidGlobalVariableName,
        saveGlobalVariable,
    } from "@/services/settings/globalVariablesSettings";

    const EMPTY_FORM = {
        id: null,
        name: "",
        value: "",
        description: "",
        valueType: "common",
    };

    export default {
        name: "GlobalVariableFormModal",
        components: { ModalComponent, Multiselect },
        emits: ["saved"],
        data() {
            return {
                form: { ...EMPTY_FORM },
                showValue: false,
                isReplacingCommonValue: false,
                validationError: "",
            };
        },
        computed: {
            isEditing() {
                return Boolean(this.form.id);
            },
            nameHasError() {
                return [
                    "settings.globalVariables.form.invalidName",
                    "settings.globalVariables.form.duplicateName",
                    "settings.globalVariables.editRestricted",
                ].includes(this.validationError);
            },
            valueHasError() {
                return [
                    "settings.globalVariables.form.valueRequired",
                    "settings.globalVariables.form.commonVariableRequired",
                ].includes(this.validationError);
            },
            isEnvironmentType() {
                return this.form.valueType === "environment";
            },
            commonVariableOptions() {
                return findCommonGlobalVariables(this.form.id).map((variable) => ({
                    label: variable.name,
                    value: variable.name,
                }));
            },
            selectedCommonVariable: {
                get() {
                    return this.form.value.match(/^\{\{global:([A-Za-z][A-Za-z0-9_]*)\}\}$/)?.[1] || "";
                },
                set(variableName) {
                    this.form.value = variableName ? `{{global:${variableName}}}` : "";
                },
            },
            modalTitle() {
                return this.isEditing
                    ? "settings.globalVariables.form.editTitle"
                    : "settings.globalVariables.form.createTitle";
            },
            placeholder() {
                return `{{global:${this.form.name || "nome"}}}`;
            },
        },
        methods: {
            open(variable = null) {
                this.form = variable ? { ...variable } : { ...EMPTY_FORM };
                this.isReplacingCommonValue = false;
                this.showValue = false;
                this.validationError = "";
                this.$refs.modal?.open();
            },
            resetForm() {
                this.form = { ...EMPTY_FORM };
                this.isReplacingCommonValue = false;
                this.validationError = "";
            },
            changeValueType() {
                this.form.value = "";
                this.isReplacingCommonValue = this.form.valueType === "common";
                this.validationError = "";
            },
            clearCommonValue() {
                this.form.value = "";
                this.isReplacingCommonValue = true;
                this.validationError = "";
                this.$nextTick(() => document.getElementById("global-variable-value")?.focus());
            },
            validate() {
                if (!isValidGlobalVariableName(this.form.name)) {
                    return "settings.globalVariables.form.invalidName";
                }
                if (!this.form.value) return "settings.globalVariables.form.valueRequired";
                const commonVariableExists = this.commonVariableOptions.some(
                    (variable) => variable.value === this.selectedCommonVariable,
                );
                if (this.isEnvironmentType && !commonVariableExists) {
                    return "settings.globalVariables.form.commonVariableRequired";
                }
                if (globalVariableNameExists(this.form.name, this.form.id)) {
                    return "settings.globalVariables.form.duplicateName";
                }
                return "";
            },
            submit() {
                this.validationError = this.validate();
                if (this.validationError) return;
                const variable = saveGlobalVariable(this.form);
                if (!variable) {
                    this.validationError = "settings.globalVariables.editRestricted";
                    return;
                }
                this.$emit("saved", variable, Boolean(this.form.id));
                this.$refs.modal?.close();
            },
        },
    };
</script>

<style scoped>
    .global-variable-form__visibility {
        display: inline-grid;
        width: 38px;
        place-items: center;
        padding: 0;
    }

    .global-variable-form__clear-value {
        display: inline-grid;
        width: 42px;
        place-items: center;
        padding: 0;
    }

    .global-variable-form__type-options {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
        gap: 0.75rem;
    }

    .global-variable-form__type-option {
        position: relative;
        cursor: pointer;
    }

    .global-variable-form__type-option input {
        position: absolute;
        opacity: 0;
    }

    .global-variable-form__type-option > span {
        display: grid;
        min-height: 88px;
        grid-template-columns: auto 1fr;
        gap: 0.25rem 0.5rem;
        align-content: center;
        padding: 0.75rem;
        border: 1px solid var(--color-border-form-control);
        border-radius: 6px;
        cursor: pointer;
    }

    .global-variable-form__type-option small {
        grid-column: 1 / -1;
        color: var(--color-text-muted);
    }

    .global-variable-form__type-option input:checked + span {
        border-color: var(--color-btn-outline-primary, #0d6efd);
        background: var(--color-bg-body-content);
    }

    .global-variable-form__type-option input:focus-visible + span {
        outline: 3px solid color-mix(in srgb, var(--color-btn-outline-primary, #0d6efd) 30%, transparent);
        outline-offset: 2px;
    }

    .alert {
        display: flex;
        align-items: flex-start;
        gap: 0.5rem;
    }

    .global-variable-form__usage {
        color: var(--color-text-muted);
        font-size: 0.78rem;
    }

    .global-variable-form__usage code {
        color: #e54782;
    }

    @media (max-width: 576px) {
        .global-variable-form__type-options {
            grid-template-columns: 1fr;
        }

        .global-variable-form__type-option > span {
            min-height: 76px;
        }

    }
</style>
