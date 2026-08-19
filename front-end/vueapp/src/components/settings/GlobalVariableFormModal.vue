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
                :class="{ 'is-invalid': validationError }"
                autocomplete="off"
                @input="validationError = ''"
            />
            <div v-if="validationError" class="invalid-feedback">{{ $t(validationError) }}</div>
            <div v-else class="form-text">{{ $t("settings.globalVariables.form.nameHint") }}</div>
        </div>

        <div v-if="isEditing" class="mb-3">
            <label class="form-label" for="global-variable-value">
                {{ $t("settings.globalVariables.form.value") }}
            </label>
            <div class="global-variable-form__secret">
                <span id="global-variable-value" class="global-variable-form__masked" aria-hidden="true">
                    ••••••••••••
                </span>
                <button
                    type="button"
                    class="btn btn-outline-secondary global-variable-form__copy"
                    :title="$t('settings.globalVariables.form.copyValue')"
                    :aria-label="$t('settings.globalVariables.form.copyValue')"
                    @click="copyValue"
                >
                    <LucideIcon icon="Copy" :size="16" />
                </button>
            </div>
        </div>

        <div v-else class="mb-3">
            <label class="form-label" for="global-variable-value">
                {{ $t("settings.globalVariables.form.value") }}
            </label>
            <div class="input-group">
                <input
                    id="global-variable-value"
                    v-model="form.value"
                    :type="showValue ? 'text' : 'password'"
                    class="form-control"
                    autocomplete="new-password"
                />
                <button
                    type="button"
                    class="btn btn-outline-secondary global-variable-form__visibility"
                    :title="$t(showValue ? 'settings.globalVariables.form.hide' : 'settings.globalVariables.form.show')"
                    @click="showValue = !showValue"
                >
                    <LucideIcon :icon="showValue ? 'EyeOff' : 'Eye'" :size="16" />
                </button>
            </div>
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

        <fieldset class="mb-3">
            <legend class="form-label mb-2">{{ $t("settings.globalVariables.form.valueType") }}</legend>
            <div class="global-variable-form__type-options">
                <label class="global-variable-form__type-option">
                    <input v-model="form.valueType" type="radio" value="common" />
                    <span>
                        <LucideIcon icon="Braces" :size="16" />
                        <strong>{{ $t("settings.globalVariables.types.common") }}</strong>
                        <small>{{ $t("settings.globalVariables.form.commonHint") }}</small>
                    </span>
                </label>
                <label class="global-variable-form__type-option">
                    <input v-model="form.valueType" type="radio" value="secret" />
                    <span>
                        <LucideIcon icon="LockKeyhole" :size="16" />
                        <strong>{{ $t("settings.globalVariables.types.secret") }}</strong>
                        <small>{{ $t("settings.globalVariables.form.secretHint") }}</small>
                    </span>
                </label>
            </div>
        </fieldset>

        <div class="global-variable-form__availability mb-3">
            <div>
                <label class="form-check-label fw-semibold" for="global-variable-environment">
                    {{ $t("settings.globalVariables.form.availableAsEnvironment") }}
                </label>
                <div class="form-text mt-1">{{ $t("settings.globalVariables.form.environmentHint") }}</div>
            </div>
            <div class="form-check form-switch m-0">
                <input
                    id="global-variable-environment"
                    v-model="form.availableAsEnvironment"
                    class="form-check-input"
                    type="checkbox"
                    role="switch"
                />
            </div>
        </div>

        <div v-if="form.valueType === 'secret'" class="alert alert-warning py-2 small" role="note">
            <LucideIcon icon="ShieldAlert" :size="16" />
            {{ $t("settings.globalVariables.form.secretUsageRestriction") }}
        </div>

        <p class="global-variable-form__usage mb-0">
            {{ $t("settings.globalVariables.form.usage") }}
            <code>{{ placeholder }}</code>
        </p>
    </ModalComponent>
</template>

<script>
    import ModalComponent from "@/components/global/ModalComponent.vue";
    import {
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
        availableAsEnvironment: false,
    };

    export default {
        name: "GlobalVariableFormModal",
        components: { ModalComponent },
        emits: ["saved"],
        data() {
            return {
                form: { ...EMPTY_FORM },
                showValue: false,
                validationError: "",
            };
        },
        computed: {
            isEditing() {
                return Boolean(this.form.id);
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
                this.showValue = false;
                this.validationError = "";
                this.$refs.modal?.open();
            },
            resetForm() {
                this.form = { ...EMPTY_FORM };
                this.validationError = "";
            },
            async copyValue() {
                try {
                    await navigator.clipboard.writeText(this.form.value);
                    this.notify("settings.globalVariables.form.valueCopied", "success");
                } catch {
                    this.notify("settings.globalVariables.form.copyFailed", "danger");
                }
            },
            notify(message, variant) {
                this.$notify({
                    title: "settings.globalVariables.title",
                    message,
                    variant,
                    icon: variant === "success" ? "Copy" : "AlertTriangle",
                });
            },
            validate() {
                if (!isValidGlobalVariableName(this.form.name)) {
                    return "settings.globalVariables.form.invalidName";
                }
                if (!this.form.value) return "settings.globalVariables.form.valueRequired";
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

    .global-variable-form__secret {
        display: flex;
        min-height: 38px;
        overflow: hidden;
        border: 1px solid var(--color-border-form-control);
        border-radius: 6px;
        background: var(--color-bg-body-content);
    }

    .global-variable-form__masked {
        display: flex;
        flex: 1;
        align-items: center;
        padding: 0.375rem 0.75rem;
        color: var(--color-text-muted);
        letter-spacing: 2px;
    }

    .global-variable-form__copy {
        display: inline-grid;
        width: 42px;
        place-items: center;
        padding: 0;
        border-width: 0 0 0 1px;
        border-radius: 0;
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

    .global-variable-form__availability {
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 1rem;
        padding: 0.75rem;
        border: 1px solid var(--color-border-form-control);
        border-radius: 6px;
    }

    .global-variable-form__availability .form-check-input {
        width: 2.75rem;
        height: 1.5rem;
        cursor: pointer;
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

        .global-variable-form__availability {
            align-items: flex-start;
        }

        .global-variable-form__availability .form-check-input {
            min-width: 2.75rem;
        }
    }
</style>
