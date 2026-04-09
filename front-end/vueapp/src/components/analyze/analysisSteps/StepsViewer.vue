<template>
    <div class="step-stepper-container">
        <button
            class="stepper-arrow stepper-arrow-left"
            @click="previousStep"
            :disabled="currentIndex === 0"
            :title="$t('analyze.previousStep')"
        >
            <i class="fas fa-chevron-left"></i>
        </button>

        <div class="step-display">
            <div class="step-badge">
                <i class="fas fa-file-alt"></i>
                {{ currentStep.name }}
            </div>
            <div class="step-indicator">{{ currentIndex + 1 }} - {{ totalSteps }}</div>
            <div class="step-dots">
                <span
                    v-for="(step, index) in steps"
                    :key="index"
                    :class="['dot', { active: index === currentIndex }]"
                ></span>
            </div>
        </div>

        <button
            class="stepper-arrow stepper-arrow-right"
            @click="nextStep"
            :disabled="currentIndex === steps.length - 1"
            :title="$t('analyze.nextStep')"
        >
            <i class="fas fa-chevron-right"></i>
        </button>
    </div>
</template>
<script>
    export default {
        name: "StepStepper",
        props: {
            steps: {
                type: Array,
                required: true,
                default: () => [],
            },
            initialStepId: {
                type: String,
                default: "",
            },
        },
        emits: ["step-changed"],
        data() {
            return {
                currentIndex: 0,
            };
        },
        computed: {
            currentStep() {
                return this.steps[this.currentIndex] || { id: "", name: "" };
            },
            totalSteps() {
                return this.steps.length;
            },
        },
        methods: {
            previousStep() {
                if (this.currentIndex > 0) {
                    this.currentIndex--;
                    this.emitStepChange();
                }
            },
            nextStep() {
                if (this.currentIndex < this.steps.length - 1) {
                    this.currentIndex++;
                    this.emitStepChange();
                }
            },
            emitStepChange() {
                this.$emit("step-changed", this.currentStep);
            },
            setInitialStep() {
                if (this.initialStepId) {
                    const index = this.steps.findIndex((s) => s.id === this.initialStepId);
                    if (index !== -1) {
                        this.currentIndex = index;
                    }
                }
            },
        },
        watch: {
            initialStepId: {
                immediate: true,
                handler() {
                    this.setInitialStep();
                },
            },
            steps: {
                immediate: true,
                handler() {
                    this.setInitialStep();
                },
            },
        },
        mounted() {
            if (this.steps.length > 0) {
                this.emitStepChange();
            }
        },
    };
</script>
<style scoped>
    .step-stepper-container {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 1rem;
        padding: 1rem;
        background: var(--color-card-content);
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    }

    .stepper-arrow {
        background: #0073e6;
        border: none;
        color: white;
        width: 40px;
        height: 40px;
        border-radius: 50%;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.3s ease;
    }

    .stepper-arrow:hover:not(:disabled) {
        background: #005bb5;
        transform: scale(1.1);
    }

    .stepper-arrow:disabled {
        background: #ccc;
        cursor: not-allowed;
        opacity: 0.5;
    }

    .step-display {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        flex: 1;
        max-width: 400px;
    }

    .step-badge {
        background: #0073e6;
        color: white;
        padding: 0.5rem 1.5rem;
        border-radius: 20px;
        font-weight: 500;
        display: flex;
        align-items: center;
        gap: 0.5rem;
        font-size: 0.95rem;
    }

    .step-indicator {
        font-size: 0.85rem;
        color: #666;
    }

    .step-dots {
        display: flex;
        gap: 0.5rem;
        align-items: center;
    }

    .dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: #ccc;
        transition: all 0.3s ease;
    }

    .dot.active {
        background: #0073e6;
        transform: scale(1.3);
    }

    @media (max-width: 768px) {
        .step-stepper-container {
            gap: 0.5rem;
            padding: 0.75rem;
        }

        .stepper-arrow {
            width: 32px;
            height: 32px;
        }

        .step-badge {
            padding: 0.4rem 1rem;
            font-size: 0.85rem;
        }

        .step-display {
            max-width: 250px;
        }
    }
</style>
