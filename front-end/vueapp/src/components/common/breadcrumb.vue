<template>
    <nav aria-label="breadcrumb">
        <ol class="breadcrumb">
            <li
                class="breadcrumb-item active"
                aria-current="page"
                v-for="(item, index) in crumbs"
                :key="index"
            >
                <router-link
                    :to="
                        item.link.queryPage
                            ? { name: item.link.to, query: { page: item.link.queryPage } }
                            : { name: item.link.to }
                    "
                >
                    {{ item.crumb }}
                </router-link>
            </li>
        </ol>
    </nav>
</template>

<script>
    export default {
        name: 'Breadcrumb',
        props: {
            crumbs: {
                required: true,
                type: Array,
                default: [],
            },
        },
        data() {
            return {
                title: 'Component Breadcrumb',
            }
        },
        components: {},
        watch: {},
        methods: {},
        computed: {},
        created() {},
        mounted() {},
        unmounted() {},
    }
</script>

<style scoped>
    .breadcrumb {
        margin-bottom: 0;
    }
</style>
