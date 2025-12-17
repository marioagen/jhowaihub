<template>
    <div class="position-fixed mt-5 end-0 p-3" style="z-index: 11">
        <div
            id="liveToast"
            :class="showToast ? 'toast fade show' : 'toast fade hide'"
            role="alert"
            aria-live="assertive"
            aria-atomic="true"
        >
            <div class="toast-header">
                <strong class="me-auto">
                    <i class="fas fa-bell bell-animated"></i>
                    {{ $t("common.notice") }}
                </strong>
                <button
                    type="button"
                    class="btn-close"
                    :title="$t('common.close')"
                    data-bs-dismiss="toast"
                    aria-label="Close"
                    @click="close"
                ></button>
            </div>
            <div :class="'toast-body ' + colorToast">
                {{ messageToast }}
            </div>
        </div>
    </div>
</template>

<script>
    export default {
        name: "ToastAlert",
        props: {
            showToast: {
                required: true,
                type: Boolean,
                default: false,
            },
            colorToast: {
                required: true,
                type: String,
                default: "",
            },
            messageToast: {
                required: true,
                type: String,
                default: "",
            },
        },
        data() {
            return {
                title: "Toast Alert",
            };
        },
        components: {},
        watch: {},
        methods: {
            close: function () {
                this.$emit("close");
            },
        },
        computed: {},
        created() {},
        mounted() {},
        unmounted() {},
    };
</script>

<style scoped>
    .toast {
        width: initial;
    }

    /* Bell animated  */
    .bell-animated {
        width: 15px;
        -webkit-animation-name: pendulum;
        animation-delay: 0s;
        -webkit-animation-duration: 1s;
        -webkit-animation-iteration-count: infinite;
        -webkit-animation-direction: linear;
        -webkit-transform-origin: 50% 0%;
        -webkit-animation-timing-function: ease-in-out;
    }

    @-moz-keyframes pendulum {
        0% {
            -webkit-transform: rotate(0deg);
        }

        10% {
            -webkit-transform: rotate(5deg);
        }

        20% {
            -webkit-transform: rotate(15deg);
        }

        30% {
            -webkit-transform: rotate(10deg);
        }

        40% {
            -webkit-transform: rotate(5deg);
        }

        50% {
            -webkit-transform: rotate(0deg);
        }

        60% {
            -webkit-transform: rotate(-5deg);
        }

        70% {
            -webkit-transform: rotate(-10deg);
        }

        80% {
            -webkit-transform: rotate(-15deg);
        }

        90% {
            -webkit-transform: rotate(-5deg);
        }

        100% {
            -webkit-transform: rotate(0deg);
        }
    }

    @-webkit-keyframes pendulum {
        0% {
            -webkit-transform: rotate(0deg);
        }

        10% {
            -webkit-transform: rotate(5deg);
        }

        20% {
            -webkit-transform: rotate(15deg);
        }

        30% {
            -webkit-transform: rotate(10deg);
        }

        40% {
            -webkit-transform: rotate(5deg);
        }

        50% {
            -webkit-transform: rotate(0deg);
        }

        60% {
            -webkit-transform: rotate(-5deg);
        }

        70% {
            -webkit-transform: rotate(-10deg);
        }

        80% {
            -webkit-transform: rotate(-15deg);
        }

        90% {
            -webkit-transform: rotate(-5deg);
        }

        100% {
            -webkit-transform: rotate(0deg);
        }
    }

    @keyframes pendulum {
        0% {
            -webkit-transform: rotate(0deg);
        }

        10% {
            -webkit-transform: rotate(5deg);
        }

        20% {
            -webkit-transform: rotate(15deg);
        }

        30% {
            -webkit-transform: rotate(10deg);
        }

        40% {
            -webkit-transform: rotate(5deg);
        }

        50% {
            -webkit-transform: rotate(0deg);
        }

        60% {
            -webkit-transform: rotate(-5deg);
        }

        70% {
            -webkit-transform: rotate(-10deg);
        }

        80% {
            -webkit-transform: rotate(-15deg);
        }

        90% {
            -webkit-transform: rotate(-5deg);
        }

        100% {
            -webkit-transform: rotate(0deg);
        }
    }
</style>
