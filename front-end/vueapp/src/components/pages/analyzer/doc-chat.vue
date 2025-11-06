<template>
    <div class="doc-chat-container">
        <button class="chat-toggle-button" @click="toggleChat" :class="{ expanded: isExpanded }">
            <i class="fas fa-comment-dots"></i>
            {{ $t("labelAskTheDoc") }}
        </button>

        <div v-if="isExpanded" class="chat-panel">
            <div class="chat-header">
                <i class="fas fa-comment-dots"></i>
                {{ $t("labelConversationWithDocument") }}
                <button class="close-button" @click="toggleChat" :title="$t('labelClose')">
                    <i class="fas fa-times"></i>
                </button>
            </div>

            <div class="chat-input-section">
                <label class="input-label">{{ $t("labelAskAI") }}</label>
                <textarea v-model="question"
                          class="chat-textarea"
                          :placeholder="$t('labelTypeYourQuestion')"
                          rows="4"
                          @input="handleInput"></textarea>

                <button v-if="question.trim()"
                        class="send-button"
                        @click="sendQuestion"
                        :disabled="isSending">
                    <i class="fas fa-paper-plane"></i>
                    {{ $t("labelSendQuestion") }}
                </button>
                <div v-if="output != ''">
                    <label class="input-label">{{ $t("labelAskAI") }}</label>
                    <textarea v-model="output"
                              class="chat-textarea"
                              :placeholder="$t('labelTypeYourQuestion')"
                              rows="4">
                    </textarea>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import api from "@/services/api";
    export default {
        name: "DocChat",
        props: {
            documentId: {
                type: Number,
                required: true,
            },
        },
        emits: ["question-sent"],
        data() {
            return {
                isExpanded: false,
                question: "",
                isSending: false,
                output: "",
            };
        },
        methods: {
            toggleChat() {
                this.isExpanded = !this.isExpanded;
            },
            handleInput() {
                // This ensures the button appears/disappears reactively
            },
            async sendQuestion() {
                if (!this.question.trim()) return;
                this.isSending = true;
                try {
                    this.$emit("show-alert-toast", {
                        msg: "Pergunta enviada com sucesso",
                        color: "toast-success",
                    });
                    const response = await api.post("/Document/input", {
                        id: this.documentId,
                        input: question,
                    });
                    this.output = response;
                } catch (error) {
                    console.error("Error sending question:", error);
                    this.$emit("show-alert-toast", {
                        msg: "Erro ao enviar pergunta",
                        color: "toast-danger",
                    });
                } finally {
                    this.isSending = false;
                }
            },
        },
    };
</script>

<style scoped>
    .doc-chat-container {
        margin-top: 1rem;
    }

    .chat-toggle-button {
        width: 100%;
        padding: 0.75rem 1rem;
        background: #f8f9fa;
        border: 2px dashed #0073e6;
        border-radius: 8px;
        color: #0073e6;
        font-size: 1rem;
        font-weight: 500;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 0.5rem;
        transition: all 0.3s ease;
    }

    .chat-toggle-button:hover {
        background: #e7f3ff;
        border-color: #005bb5;
    }

    .chat-toggle-button.expanded {
        display: none;
    }

    .chat-panel {
        background: white;
        border: 2px solid #0073e6;
        border-radius: 8px;
        padding: 1rem;
        box-shadow: 0 4px 12px rgba(0, 115, 230, 0.15);
        animation: slideDown 0.3s ease;
    }

    @keyframes slideDown {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    .chat-header {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        color: #0073e6;
        font-weight: 600;
        margin-bottom: 1rem;
        padding-bottom: 0.75rem;
        border-bottom: 1px solid #e0e0e0;
        position: relative;
    }

    .close-button {
        position: absolute;
        right: 0;
        background: transparent;
        border: none;
        color: #666;
        font-size: 1.2rem;
        cursor: pointer;
        padding: 0.25rem;
        transition: color 0.3s ease;
    }

    .close-button:hover {
        color: #333;
    }

    .chat-input-section {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
    }

    .input-label {
        font-size: 0.9rem;
        color: #333;
        font-weight: 500;
        margin: 0;
    }

    .chat-textarea {
        width: 100%;
        padding: 0.75rem;
        border: 1px solid #ddd;
        border-radius: 6px;
        font-size: 0.9rem;
        font-family: inherit;
        resize: vertical;
        transition: border-color 0.3s ease;
    }

    .chat-textarea:focus {
        outline: none;
        border-color: #0073e6;
    }

    .chat-textarea::placeholder {
        color: #999;
    }

    .send-button {
        align-self: flex-start;
        padding: 0.75rem 1.5rem;
        background: #0073e6;
        border: none;
        border-radius: 6px;
        color: white;
        font-size: 0.95rem;
        font-weight: 500;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 0.5rem;
        transition: all 0.3s ease;
    }

    .send-button:hover:not(:disabled) {
        background: #005bb5;
        transform: translateY(-1px);
        box-shadow: 0 4px 8px rgba(0, 115, 230, 0.3);
    }

    .send-button:disabled {
        opacity: 0.6;
        cursor: not-allowed;
    }

    @media (max-width: 768px) {
        .chat-panel {
            padding: 0.75rem;
        }

        .chat-textarea {
            padding: 0.6rem;
            font-size: 0.85rem;
        }

        .send-button {
            padding: 0.65rem 1.25rem;
            font-size: 0.9rem;
        }
    }
</style>
