<template>
    <main>
        <div class="container-fluid scroll-area manage-user mx-2">
            <h1>Dashboard</h1>
            <TokensGraph />
        </div>
    </main>
</template>

<script>
    import TokensGraph from '@/components/dashboard/graphs/TokensGraph.vue';
    export default {
        components: {
            TokensGraph,
        }
    }
</script>