<script lang="ts">
	import type { PageData } from './$types';
	let { data }: { data: PageData } = $props();
</script>

<main>
	<p>Session: {data.id}</p>
</main>
