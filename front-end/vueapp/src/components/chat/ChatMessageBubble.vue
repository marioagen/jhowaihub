<template>
    <div
        class="chat-bubble"
        :class="[`chat-bubble--${message.role}`, { 'chat-bubble--typing': typing }]"
    >
        <div class="chat-bubble__meta">
            <LucideIcon
                :icon="roleIcon"
                :size="16"
            />
            <span>{{ roleLabel }}</span>
            <small>{{ formattedTime }}</small>
        </div>

        <div
            v-if="message.attachments?.length"
            class="chat-bubble__attachments"
        >
            <div
                v-for="file in message.attachments"
                :key="file.name"
                class="chat-attachment"
            >
                <LucideIcon
                    icon="Paperclip"
                    :size="14"
                />
                <span>{{ file.name }}</span>
                <small>{{ formatSize(file.size) }}</small>
            </div>
        </div>

        <div
            v-if="message.content"
            class="chat-bubble__content chat-markdown"
            v-html="renderedContent"
        ></div>

        <div
            v-if="message.documentResponse"
            class="chat-document-card"
        >
            <div class="chat-document-card__header">
                <LucideIcon
                    icon="FileText"
                    :size="18"
                />
                <div>
                    <strong>{{ message.documentResponse.title }}</strong>
                    <small>{{ formatLabel }}</small>
                </div>
            </div>
            <p
                v-if="message.documentResponse.preview"
                class="mb-2 chat-document-card__preview-text small"
            >
                {{ message.documentResponse.preview }}
            </p>
            <pre
                v-if="message.documentResponse.content"
                class="chat-document-card__preview"
            >{{ message.documentResponse.content }}</pre>
            <div class="chat-document-card__actions">
                <button
                    type="button"
                    class="btn btn-sm btn-outline-primary"
                    @click="$emit('export-document', message.documentResponse)"
                >
                    <LucideIcon
                        icon="Download"
                        :size="14"
                    />
                    {{ $t("chat.exportDocument") }}
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "ChatMessageBubble",
        props: {
            message: {
                type: Object,
                required: true,
            },
            typing: {
                type: Boolean,
                default: false,
            },
        },
        emits: ["export-document"],
        computed: {
            roleIcon() {
                if (this.message.role === "user") return "User";
                return "Bot";
            },
            roleLabel() {
                if (this.message.role === "user") return this.$t("chat.roles.user");
                return this.$t("chat.roles.assistant");
            },
            formattedTime() {
                if (!this.message.timestamp) return "";
                return new Date(this.message.timestamp).toLocaleTimeString([], {
                    hour: "2-digit",
                    minute: "2-digit",
                });
            },
            formatLabel() {
                const format = this.message.documentResponse?.format || "file";
                return format.toUpperCase();
            },
            renderedContent() {
                return this.formatMarkdown(this.message.content || "");
            },
        },
        methods: {
            formatSize(bytes) {
                if (!bytes) return "";
                if (bytes < 1024) return `${bytes} B`;
                if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
                return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
            },
            formatMarkdown(text) {
                return text
                    .replace(/&/g, "&amp;")
                    .replace(/</g, "&lt;")
                    .replace(/>/g, "&gt;")
                    .replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>")
                    .replace(/\*(.+?)\*/g, "<em>$1</em>")
                    .replace(/^> (.+)$/gm, "<blockquote>$1</blockquote>")
                    .replace(/\n/g, "<br>");
            },
        },
    };
</script>

<style scoped>
    .chat-bubble {
        max-width: min(720px, 92%);
        padding: 0.85rem 1rem;
        border-radius: 12px;
        border: 1px solid var(--color-border-form-control);
        background: var(--color-card-content);
        color: var(--color-body-content);
    }

    .chat-bubble--user {
        margin-left: auto;
        background: var(--color-bg-btn-primary, #0d6efd);
        color: #fff;
        border-color: transparent;
    }

    .chat-bubble--user .chat-bubble__meta,
    .chat-bubble--user .chat-attachment {
        color: rgba(255, 255, 255, 0.9);
    }

    .chat-bubble--assistant {
        margin-right: auto;
    }

    .chat-bubble__meta {
        display: flex;
        align-items: center;
        gap: 0.35rem;
        font-size: 0.75rem;
        color: var(--color-text-muted);
        margin-bottom: 0.5rem;
    }

    .chat-bubble__meta small {
        margin-left: auto;
    }

    .chat-bubble__attachments {
        display: flex;
        flex-direction: column;
        gap: 0.35rem;
        margin-bottom: 0.5rem;
    }

    .chat-attachment {
        display: flex;
        align-items: center;
        gap: 0.35rem;
        font-size: 0.85rem;
        padding: 0.35rem 0.5rem;
        border-radius: 8px;
        background: var(--color-bg-body-content);
        border: 1px solid var(--color-border-form-control);
    }

    .chat-bubble--user .chat-attachment {
        background: rgba(255, 255, 255, 0.15);
    }

    .chat-bubble__content {
        font-size: 0.92rem;
        line-height: 1.55;
        word-break: break-word;
        color: inherit;
    }

    .chat-markdown :deep(blockquote) {
        margin: 0.5rem 0;
        padding: 0.35rem 0 0.35rem 0.75rem;
        border-left: 3px solid var(--color-border-form-control);
        color: var(--color-text-muted);
    }

    .chat-document-card {
        margin-top: 0.75rem;
        padding: 0.75rem;
        border-radius: 10px;
        border: 1px dashed var(--color-border-form-control);
        background: var(--color-bg-body-content);
        color: var(--color-body-content);
    }

    .chat-document-card__preview-text {
        color: var(--color-text-muted);
    }

    .chat-document-card__header {
        display: flex;
        gap: 0.5rem;
        align-items: flex-start;
        margin-bottom: 0.5rem;
    }

    .chat-document-card__header strong {
        display: block;
        font-size: 0.9rem;
    }

    .chat-document-card__preview {
        max-height: 220px;
        overflow: auto;
        font-size: 0.78rem;
        white-space: pre-wrap;
        background: var(--color-bg-form-control);
        color: var(--color-body-content);
        border: 1px solid var(--color-border-form-control);
        border-radius: 8px;
        padding: 0.65rem;
        margin-bottom: 0.5rem;
    }

    .chat-document-card__actions {
        display: flex;
        gap: 0.5rem;
    }

    .chat-bubble--typing .chat-bubble__content::after {
        content: "▋";
        animation: blink 1s step-end infinite;
    }

    @keyframes blink {
        50% {
            opacity: 0;
        }
    }
</style>
