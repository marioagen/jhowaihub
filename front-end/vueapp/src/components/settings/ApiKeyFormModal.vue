<template>

    <ModalComponent

        ref="modal"

        id="api-key-form-modal"

        :title="modalTitle"

        save-text="common.save"

        @save="submit"

        @cancel="resetForm"

    >

        <div class="mb-3">

            <label

                class="form-label"

                for="api-key-name"

            >

                {{ $t("settings.apiKeys.form.nameLabel") }}

            </label>

            <input

                id="api-key-name"

                v-model="form.name"

                type="text"

                class="form-control"

                :placeholder="$t('settings.apiKeys.form.namePlaceholder')"

            />

        </div>



        <div class="mb-1">

            <label class="form-label">

                {{ $t("settings.apiKeys.form.valueLabel") }}

            </label>

            <div class="api-key-form__value-row">

                <code class="api-key-form__value">{{ form.value }}</code>

                <button

                    type="button"

                    class="btn btn-outline-primary btn-sm text-nowrap"

                    @click="generateValue"

                >

                    <LucideIcon

                        icon="RefreshCw"

                        :size="14"

                    />

                    {{ $t("settings.apiKeys.form.generate") }}

                </button>

            </div>

        </div>

        <p class="text-muted small mb-0">{{ $t("settings.apiKeys.form.generateHint") }}</p>

    </ModalComponent>

</template>



<script>

    import ModalComponent from "@/components/global/ModalComponent.vue";

    import { createApiKey, generateApiKeyValue } from "@/services/settings/apiKeysSettings";



    export default {

        name: "ApiKeyFormModal",

        components: { ModalComponent },

        emits: ["created"],

        data() {

            return {

                form: {

                    name: "",

                    value: "",

                },

            };

        },

        computed: {

            modalTitle() {

                return "settings.apiKeys.form.title";

            },

        },

        methods: {

            open() {

                this.resetForm();

                this.$refs.modal?.open();

            },

            close() {

                this.$refs.modal?.close();

            },

            resetForm() {

                this.form = {

                    name: "",

                    value: generateApiKeyValue(),

                };

            },

            generateValue() {

                this.form.value = generateApiKeyValue();

            },

            submit() {

                if (!this.form.name.trim()) {

                    this.$notify({

                        title: "settings.apiKeys.title",

                        message: "settings.apiKeys.form.nameRequired",

                        variant: "warning",

                        icon: "TriangleAlert",

                    });

                    return;

                }

                const entry = createApiKey({

                    name: this.form.name,

                    value: this.form.value,

                });

                this.$emit("created", entry);

                this.close();

            },

        },

    };

</script>



<style scoped>

    .api-key-form__value-row {

        display: flex;

        gap: 0.5rem;

        align-items: center;

    }



    .api-key-form__value {

        flex: 1;

        min-width: 0;

        display: block;

        padding: 0.5rem 0.65rem;

        border-radius: 6px;

        background: var(--color-bg-body-content);

        border: 1px solid var(--color-border-form-control);

        font-size: 0.78rem;

        color: var(--color-body-content);

        word-break: break-all;

        user-select: all;

    }

</style>


