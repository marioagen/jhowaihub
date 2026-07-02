<template>

    <OffcanvasComponent

        ref="offcanvas"

        id="chat-settings-offcanvas"

        placement="end"

        :label-id="'chat-settings-label'"

    >

        <template #header>

            <div class="offcanvas-header">

                <h5

                    id="chat-settings-label"

                    class="offcanvas-title"

                >

                    {{ $t("chat.settings.title") }}

                </h5>

                <button

                    type="button"

                    class="btn-close"

                    @click="close"

                ></button>

            </div>

        </template>



        <p class="text-muted small">{{ $t("chat.settings.subtitle") }}</p>



        <div class="mb-3">

            <label class="form-label fw-semibold">{{ $t("chat.settings.scopes.chat") }}</label>

            <select

                v-model="localChatModel"

                class="form-select form-select-sm"

            >

                <option

                    v-for="model in models"

                    :key="model.id"

                    :value="model.id"

                >

                    {{ model.label }}

                </option>

            </select>

        </div>



        <router-link

            to="/settings?tab=llm-models"

            class="chat-settings-link small"

            @click="close"

        >

            <LucideIcon

                icon="Settings"

                :size="14"

            />

            {{ $t("chat.settings.openPlatformSettings") }}

        </router-link>



        <div class="alert alert-primary small mb-0 mt-3 chat-settings-alert">

            <LucideIcon

                icon="Info"

                :size="14"

            />

            {{ $t("chat.settings.simulationNotice") }}

        </div>



        <div class="mt-3 d-flex gap-2">

            <button

                type="button"

                class="btn btn-primary btn-sm"

                @click="save"

            >

                {{ $t("common.save") }}

            </button>

            <button

                type="button"

                class="btn btn-outline-secondary btn-sm"

                @click="close"

            >

                {{ $t("common.cancel") }}

            </button>

        </div>

    </OffcanvasComponent>

</template>



<script>

    import OffcanvasComponent from "@/components/global/OffcanvasComponent.vue";

    import { DEFAULT_MODELS } from "@/services/chat/chatConstants";



    export default {

        name: "ChatSettingsOffcanvas",

        components: { OffcanvasComponent },

        props: {

            settings: {

                type: Object,

                required: true,

            },

        },

        emits: ["save"],

        data() {

            return {

                models: DEFAULT_MODELS,

                localChatModel: "",

            };

        },

        watch: {

            settings: {

                immediate: true,

                deep: true,

                handler(value) {

                    this.localChatModel = value.models?.chat || "";

                },

            },

        },

        methods: {

            open() {

                this.$refs.offcanvas?.open();

            },

            close() {

                this.$refs.offcanvas?.close();

            },

            save() {

                this.$emit("save", {

                    ...this.settings,

                    models: {

                        ...this.settings.models,

                        chat: this.localChatModel,

                    },

                });

                this.close();

            },

        },

    };

</script>



<style scoped>

    :deep(.offcanvas) {

        background-color: var(--color-card-content) !important;

        color: var(--color-body-content);

        border-left: 1px solid var(--color-border-form-control);

    }



    :deep(.offcanvas-title) {

        color: var(--color-heading-title, var(--color-body-content));

    }



    .chat-settings-alert {

        display: flex;

        align-items: flex-start;

        gap: 0.35rem;

    }



    .chat-settings-link {

        display: inline-flex;

        align-items: center;

        gap: 0.35rem;

        color: var(--color-btn-outline-primary);

        text-decoration: none;

    }



    .chat-settings-link:hover {

        text-decoration: underline;

    }

</style>


