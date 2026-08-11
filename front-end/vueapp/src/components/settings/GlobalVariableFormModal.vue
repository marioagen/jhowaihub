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

        <div class="mb-3">
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

    const EMPTY_FORM = { id: null, name: "", value: "", description: "" };

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
            modalTitle() {
                return this.form.id
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

    .global-variable-form__usage {
        color: var(--color-text-muted);
        font-size: 0.78rem;
    }

    .global-variable-form__usage code {
        color: #e54782;
    }
</style>
