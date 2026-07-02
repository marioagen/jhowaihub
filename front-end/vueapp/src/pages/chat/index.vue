<template>
    <main class="chat-page">
        <div
            v-if="mobileSidebarOpen"
            class="chat-mobile-backdrop"
            @click="mobileSidebarOpen = false"
        ></div>

        <div class="chat-page__shell">
            <ChatSessionSidebar
                :sessions="sortedSessions"
                :active-session-id="activeSession?.id"
                :mobile-open="mobileSidebarOpen"
                @new-session="createNewSession"
                @select-session="selectSession"
                @delete-session="deleteSession"
                @generate-demo="generateDemoConversation"
            />

            <section class="chat-workspace">
                <header class="chat-workspace__header">
                    <div class="d-flex align-items-center gap-2">
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm d-md-none"
                            @click="mobileSidebarOpen = !mobileSidebarOpen"
                        >
                            <LucideIcon
                                icon="Menu"
                                :size="18"
                            />
                        </button>
                        <div>
                            <h5 class="mb-0 fw-bold">{{ $t("chat.title") }}</h5>
                            <small class="text-muted">{{ $t("chat.subtitle") }}</small>
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        <button
                            type="button"
                            class="btn btn-outline-secondary btn-sm"
                            @click="openSettings"
                        >
                            <LucideIcon
                                icon="Settings"
                                :size="16"
                            />
                            <span class="d-none d-lg-inline ms-1">{{ $t("chat.settings.button") }}</span>
                        </button>
                    </div>
                </header>

                <div class="chat-workspace__toolbar">
                    <div class="row g-2 align-items-end">
                        <div class="col-md-5">
                            <label class="form-label small mb-1">{{ $t("chat.agentLabel") }}</label>
                            <select
                                v-model="selectedAgentId"
                                class="form-select form-select-sm"
                                @change="onAgentChanged"
                            >
                                <option
                                    v-for="agent in agents"
                                    :key="agent.id"
                                    :value="agent.id"
                                >
                                    {{ agent.name }}
                                </option>
                            </select>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small mb-1">{{ $t("chat.modelLabel") }}</label>
                            <select
                                v-model="selectedModel"
                                class="form-select form-select-sm"
                                @change="persistActiveSessionMeta"
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
                        <div class="col-md-3">
                            <span class="chat-simulation-badge w-100 py-2">
                                <LucideIcon
                                    icon="CloudOff"
                                    :size="14"
                                />
                                {{ $t("chat.simulationMode") }}
                            </span>
                        </div>
                    </div>
                    <p
                        v-if="activeAgent"
                        class="text-muted small mb-0 mt-2"
                    >
                        {{ activeAgent.description }}
                    </p>
                </div>

                <div
                    ref="messagesContainer"
                    class="chat-workspace__messages"
                >
                    <div
                        v-if="activeSession && !activeSession.messages.length && !isTyping"
                        class="chat-empty-hint"
                    >
                        <LucideIcon
                            icon="MessageCircleMore"
                            :size="32"
                        />
                        <p class="mb-0">{{ $t("chat.emptyHint") }}</p>
                    </div>

                    <ChatMessageBubble
                        v-for="message in activeSession?.messages || []"
                        :key="message.id"
                        :message="message"
                        @export-document="exportDocument"
                    />
                    <ChatMessageBubble
                        v-if="isTyping"
                        :message="{ role: 'assistant', content: $t('chat.typing') }"
                        typing
                    />
                </div>

                <ChatComposer
                    :disabled="!activeSession || isTyping"
                    @send="handleSend"
                />
            </section>
        </div>

        <ChatSettingsOffcanvas
            ref="settingsPanel"
            :settings="settings"
            @save="saveSettings"
        />
    </main>
</template>

<script>
    import ChatSessionSidebar from "@/components/chat/ChatSessionSidebar.vue";
    import ChatMessageBubble from "@/components/chat/ChatMessageBubble.vue";
    import ChatComposer from "@/components/chat/ChatComposer.vue";
    import ChatSettingsOffcanvas from "@/components/chat/ChatSettingsOffcanvas.vue";
    import { DEFAULT_MODELS } from "@/services/chat/chatConstants";
    import {
        createSession,
        loadActiveSessionId,
        loadAgents,
        loadSessions,
        loadSettings,
        saveActiveSessionId,
        saveSessions,
        saveSettings as persistChatSettings,
    } from "@/services/chat/chatStorage";
    import {
        buildDemoConversation,
        buildDemoSessionTitle,
        simulateAssistantReply,
    } from "@/services/chat/simulatedChat";

    export default {
        name: "ChatPage",
        components: {
            ChatSessionSidebar,
            ChatMessageBubble,
            ChatComposer,
            ChatSettingsOffcanvas,
        },
        data() {
            return {
                sessions: [],
                agents: [],
                settings: { models: {} },
                models: DEFAULT_MODELS,
                activeSessionId: loadActiveSessionId(),
                selectedAgentId: "doc-analyst",
                selectedModel: DEFAULT_MODELS[0]?.id || "gpt-4o",
                isTyping: false,
                mobileSidebarOpen: false,
            };
        },
        computed: {
            sortedSessions() {
                return [...this.sessions].sort(
                    (a, b) => new Date(b.updatedAt) - new Date(a.updatedAt),
                );
            },
            activeSession() {
                return this.sessions.find((s) => s.id === this.activeSessionId) || null;
            },
            activeAgent() {
                return this.agents.find((a) => a.id === this.selectedAgentId) || null;
            },
        },
        mounted() {
            this.agents = loadAgents();
            this.sessions = loadSessions();
            loadSettings().then((settings) => {
                this.settings = settings;
                if (settings.availableModels?.length) {
                    this.models = settings.availableModels;
                }
                if (!this.activeSession) {
                    this.selectedModel = settings.models?.chat || this.selectedModel;
                }
            });
            if (this.activeSession) {
                this.selectedAgentId = this.activeSession.agentId;
                this.selectedModel = this.activeSession.model;
            } else if (this.sessions.length) {
                this.selectSession(this.sessions[0].id);
            } else {
                this.createNewSession();
            }
        },
        methods: {
            persist() {
                saveSessions(this.sessions);
            },
            createNewSession() {
                const session = createSession({
                    agentId: this.selectedAgentId,
                    model: this.selectedModel,
                });
                this.sessions.unshift(session);
                this.selectSession(session.id);
                this.persist();
                this.mobileSidebarOpen = false;
            },
            selectSession(sessionId) {
                this.activeSessionId = sessionId;
                saveActiveSessionId(sessionId);
                const session = this.activeSession;
                if (session) {
                    this.selectedAgentId = session.agentId;
                    this.selectedModel = session.model;
                }
                this.mobileSidebarOpen = false;
                this.$nextTick(this.scrollToBottom);
            },
            deleteSession(sessionId) {
                this.sessions = this.sessions.filter((s) => s.id !== sessionId);
                if (this.activeSessionId === sessionId) {
                    const next = this.sessions[0]?.id || null;
                    this.activeSessionId = next;
                    saveActiveSessionId(next);
                    if (!next) {
                        this.createNewSession();
                    }
                }
                this.persist();
            },
            onAgentChanged() {
                this.persistActiveSessionMeta();
            },
            persistActiveSessionMeta() {
                if (!this.activeSession) return;
                this.activeSession.agentId = this.selectedAgentId;
                this.activeSession.model = this.selectedModel;
                this.activeSession.updatedAt = new Date().toISOString();
                this.persist();
            },
            async handleSend({ content, attachments }) {
                if (!this.activeSession) {
                    this.createNewSession();
                }

                const userMessage = {
                    id: crypto.randomUUID(),
                    role: "user",
                    content,
                    attachments: attachments.length ? attachments : undefined,
                    timestamp: new Date().toISOString(),
                };

                this.activeSession.messages.push(userMessage);
                if (this.activeSession.messages.length === 1) {
                    this.activeSession.title = content.slice(0, 48) || this.$t("chat.newSession");
                }
                this.activeSession.updatedAt = userMessage.timestamp;
                this.persist();
                this.scrollToBottom();

                this.isTyping = true;
                const reply = simulateAssistantReply({
                    userMessage: content,
                    attachments,
                    agent: this.activeAgent,
                    model: this.selectedModel,
                });

                await new Promise((resolve) => setTimeout(resolve, reply.delayMs));

                this.activeSession.messages.push({
                    id: crypto.randomUUID(),
                    role: "assistant",
                    content: reply.content,
                    documentResponse: reply.documentResponse,
                    timestamp: new Date().toISOString(),
                });
                this.activeSession.updatedAt = new Date().toISOString();
                this.isTyping = false;
                this.persist();
                this.scrollToBottom();
            },
            generateDemoConversation() {
                const agentId = this.selectedAgentId || "doc-analyst";
                const session = createSession({
                    agentId,
                    model: this.selectedModel,
                    title: buildDemoSessionTitle(agentId),
                });
                session.messages = buildDemoConversation(agentId);
                session.updatedAt = new Date().toISOString();
                this.sessions.unshift(session);
                this.selectSession(session.id);
                this.persist();
                this.$notify({
                    title: this.$t("chat.title"),
                    message: this.$t("chat.demoGenerated"),
                    variant: "success",
                    icon: "check",
                });
            },
            openSettings() {
                this.$refs.settingsPanel?.open();
            },
            saveSettings(nextSettings) {
                if (!this.$store.state.userProfile.isAdmin) {
                    this.settings = nextSettings;
                    this.selectedModel = nextSettings.models.chat;
                    this.persistActiveSessionMeta();
                    this.$notify({
                        title: this.$t("chat.settings.title"),
                        message: this.$t("settings.llmModels.readOnlyNotice"),
                        variant: "warning",
                        icon: "TriangleAlert",
                    });
                    return;
                }
                persistChatSettings(nextSettings).then((saved) => {
                    this.settings = saved;
                    this.selectedModel = saved.models.chat;
                    this.persistActiveSessionMeta();
                    this.$notify({
                        title: this.$t("chat.settings.title"),
                        message: this.$t("chat.settings.saved"),
                        variant: "success",
                        icon: "check",
                    });
                });
            },
            exportDocument(documentResponse) {
                const content =
                    documentResponse.content ||
                    documentResponse.preview ||
                    documentResponse.title;
                const blob = new Blob([content], { type: "text/plain;charset=utf-8" });
                const url = URL.createObjectURL(blob);
                const link = document.createElement("a");
                link.href = url;
                link.download = documentResponse.title || "documento-woopi.txt";
                link.click();
                URL.revokeObjectURL(url);
            },
            scrollToBottom() {
                const container = this.$refs.messagesContainer;
                if (container) {
                    container.scrollTop = container.scrollHeight;
                }
            },
        },
    };
</script>

<style scoped>
    .chat-page {
        display: flex !important;
        flex-direction: column !important;
        flex-wrap: nowrap !important;
        width: 100%;
        height: calc(100vh - 58px) !important;
        max-height: calc(100vh - 58px);
        min-height: calc(100vh - 58px);
        overflow: hidden;
        background: var(--color-bg-body-content);
        color: var(--color-body-content);
    }

    .chat-page__shell {
        display: flex;
        flex: 1 1 auto;
        width: 100%;
        min-height: 0;
        height: 100%;
    }

    .chat-workspace {
        flex: 1 1 auto;
        width: 100%;
        min-width: 0;
        min-height: 0;
        display: flex;
        flex-direction: column;
        background: var(--color-card-content);
    }

    .chat-workspace__header {
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: space-between;
        gap: 0.75rem;
        padding: 0.85rem 1rem;
        border-bottom: 1px solid var(--color-border-form-control);
    }

    .chat-workspace__header h5 {
        color: var(--color-heading-title, var(--color-body-content));
    }

    .chat-workspace__toolbar {
        flex-shrink: 0;
        padding: 0.75rem 1rem;
        border-bottom: 1px solid var(--color-border-form-control);
        background: var(--color-bg-body-content);
    }

    .chat-simulation-badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        gap: 0.35rem;
        border-radius: 0.375rem;
        font-size: 0.75rem;
        font-weight: 600;
        border: 1px solid var(--color-border-form-control);
        background: var(--color-bg-primary-badge);
        color: var(--color-text-primary-badge);
    }

    .chat-workspace__messages {
        flex: 1 1 auto;
        min-height: 0;
        overflow-y: auto;
        padding: 1rem;
        display: flex;
        flex-direction: column;
        gap: 0.85rem;
        background: var(--color-bg-body-content);
    }

    .chat-workspace :deep(.chat-composer) {
        flex-shrink: 0;
    }

    .chat-empty-hint {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 0.75rem;
        flex: 1 1 auto;
        min-height: 0;
        text-align: center;
        color: var(--color-text-muted);
        font-size: 0.9rem;
        padding: 1rem;
    }

    .chat-empty-hint :deep(svg) {
        color: var(--color-btn-outline-primary);
    }

    .chat-mobile-backdrop {
        position: fixed;
        inset: 58px 0 0 0;
        background: rgba(0, 0, 0, 0.35);
        z-index: 1035;
    }

    @media (max-width: 768px) {
        .chat-workspace__header,
        .chat-workspace__toolbar {
            padding-left: 0.75rem;
            padding-right: 0.75rem;
        }
    }
</style>
