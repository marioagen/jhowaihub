<template>
    <div class="chat-composer">
        <div
            v-if="pendingFiles.length"
            class="chat-composer__files"
        >
            <div
                v-for="(file, index) in pendingFiles"
                :key="`${file.name}-${index}`"
                class="chat-composer__file"
            >
                <LucideIcon
                    icon="File"
                    :size="14"
                />
                <span>{{ file.name }}</span>
                <button
                    type="button"
                    class="btn btn-link btn-sm p-0"
                    @click="removeFile(index)"
                >
                    <LucideIcon
                        icon="X"
                        :size="14"
                    />
                </button>
            </div>
        </div>

        <div class="chat-composer__row">
            <input
                ref="fileInput"
                type="file"
                class="d-none"
                multiple
                @change="onFilesSelected"
            />
            <button
                type="button"
                class="btn btn-outline-secondary chat-composer__attach"
                :title="$t('chat.attachDocument')"
                :disabled="disabled"
                @click="$refs.fileInput.click()"
            >
                <LucideIcon
                    icon="Paperclip"
                    :size="18"
                />
            </button>
            <textarea
                v-model="draft"
                class="form-control chat-composer__input"
                rows="2"
                :placeholder="$t('chat.inputPlaceholder')"
                :disabled="disabled"
                @keydown.enter.exact.prevent="send"
            ></textarea>
            <button
                type="button"
                class="btn btn-primary chat-composer__send"
                :disabled="disabled || !canSend"
                @click="send"
            >
                <LucideIcon
                    v-if="!disabled"
                    icon="Send"
                    :size="18"
                />
                <span
                    v-else
                    class="spinner-border spinner-border-sm"
                ></span>
            </button>
        </div>
    </div>
</template>

<script>
    export default {
        name: "ChatComposer",
        props: {
            disabled: {
                type: Boolean,
                default: false,
            },
        },
        emits: ["send"],
        data() {
            return {
                draft: "",
                pendingFiles: [],
            };
        },
        computed: {
            canSend() {
                return this.draft.trim().length > 0 || this.pendingFiles.length > 0;
            },
        },
        methods: {
            onFilesSelected(event) {
                const files = Array.from(event.target.files || []);
                this.pendingFiles.push(
                    ...files.map((file) => ({
                        name: file.name,
                        type: file.type,
                        size: file.size,
                    })),
                );
                event.target.value = "";
            },
            removeFile(index) {
                this.pendingFiles.splice(index, 1);
            },
            send() {
                if (!this.canSend || this.disabled) return;
                this.$emit("send", {
                    content: this.draft.trim(),
                    attachments: [...this.pendingFiles],
                });
                this.draft = "";
                this.pendingFiles = [];
            },
        },
    };
</script>

<style scoped>
    .chat-composer {
        border-top: 1px solid var(--color-border-form-control);
        padding: 0.75rem;
        background: var(--color-card-content);
        color: var(--color-body-content);
    }

    .chat-composer__files {
        display: flex;
        flex-wrap: wrap;
        gap: 0.35rem;
        margin-bottom: 0.5rem;
    }

    .chat-composer__file {
        display: inline-flex;
        align-items: center;
        gap: 0.35rem;
        padding: 0.25rem 0.5rem;
        border-radius: 999px;
        background: var(--color-bg-body-content);
        border: 1px solid var(--color-border-form-control);
        color: var(--color-body-content);
        font-size: 0.8rem;
    }

    .chat-composer__file .btn-link {
        color: var(--color-text-muted);
    }

    .chat-composer__file .btn-link:hover {
        color: var(--color-bg-btn-danger);
    }

    .chat-composer__row {
        display: flex;
        gap: 0.5rem;
        align-items: flex-end;
    }

    .chat-composer__input {
        resize: none;
        min-height: 44px;
        max-height: 140px;
    }

    .chat-composer__attach,
    .chat-composer__send {
        flex-shrink: 0;
        width: 44px;
        height: 44px;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        padding: 0;
    }
</style>
