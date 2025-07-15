<template>
    <div class="dropdown position-static">
        <img
            src="@/assets/img/menu-dots.svg"
            alt="Menu"
            class="menu-icon"
            data-bs-toggle="dropdown"
            aria-expanded="false"
        />
        <ul class="dropdown-menu">
            <li v-for="(option, index) in menuOptions.options" :key="index">
                <a
                    class="dropdown-item text-sm"
                    :class="option.color"
                    href="#"
                    @click.prevent="handleMenuAction(option)"
                >
                    <span class="option-icon">
                        <img v-if="option.icon" :src="option.icon" />
                    </span>
                    {{ option.label }}
                </a>
            </li>
        </ul>
    </div>
</template>
<script>
    export default {
        name: 'DropdownMenu',
        props: {
            menuOptions: {
                required: true,
                type: Object,
                default: {},
            },
        },
        data() {
            return {
                title: 'Component DropdownMenu',
            }
        },
        methods: {
            handleMenuAction(option) {
                this.$emit('action', option)
            },
        },
    }
</script>
<style>
    .menu-icon {
        cursor: pointer;
        width: 16px;
        height: 16px;
    }

    .option-icon {
        display: inline-block;
        width: 20px;
        text-align: center;
        vertical-align: middle;
    }

    .option-icon img {
        height: 16px;
        width: 16px;
        vertical-align: baseline;
    }
</style>
